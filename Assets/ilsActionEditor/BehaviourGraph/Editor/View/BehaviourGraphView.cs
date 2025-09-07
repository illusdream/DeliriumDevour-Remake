using System;
using System.Collections.Generic;
using System.Linq;
using ilsActionEditor.Editor;
using ilsActionEditor.Node;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using XNodeEditor;

namespace ilsActionEditor
{
    public class BehaviourGraphView : GraphView
    {
        
        private BaseBehaviourGraph _currentGraph;

        public BaseBehaviourGraph CurrentGraph
        {
            get=> _currentGraph;
            set=> _currentGraph=value;
        }
        
        private BehaviourGraphLayout _currentGraphLayout;
        public BehaviourGraphLayout CurrentGraphLayout => _currentGraphLayout;
        
        public List<BaseNodeView> NodesViews=new List<BaseNodeView>();
        public Dictionary<BaseBehaviourNode,BaseNodeView> NodeDictionary = new Dictionary<BaseBehaviourNode,BaseNodeView>();
        public List<BaseStickyNoteView> StickyNoteViews=new List<BaseStickyNoteView>();
        public List<Edge> Edges=new List<Edge>();
        
        public BehaviourGraphEditorWindow _editorWindow;

        private BehaviourSearchMenuWindowProvider BehaviourSearchMenuWindowProviderInstance;
        public BehaviourGraphView()
        {
            InitializeManipulator();
            InitializeSearchMenuWindow();
            
            graphViewChanged += _GraphViewChanged;
        }



        private GraphViewChange _GraphViewChanged(GraphViewChange graphviewchange)
        {
            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    //这个时候把链接给建立了
                    if (edge.input is BasePortPointView inputView && edge.output is BasePortPointView outputView)
                    {
                        if (inputView.nodePort != null && outputView.nodePort != null)
                        {
                            Edges.Add(edge);
                            inputView.nodePort.Connect(outputView.nodePort);
                        }
                    }
                }
            }

            if (graphviewchange.elementsToRemove != null)
            {
                foreach (var element in graphviewchange.elementsToRemove)
                {
                    if (element is BaseEdgeView edgeView)
                    {
                        //这个时候把链接给关闭了
                        if (edgeView.input is BasePortPointView inputView && edgeView.output is BasePortPointView outputView)
                        {
                            if (inputView.nodePort != null && outputView.nodePort != null)
                            {
                                Edges.Remove(edgeView);
                                inputView.nodePort.Disconnect(outputView.nodePort);
                            }
                        }
                    }

                    if (element is BaseNodeView nodeView)
                    {
                        DeleteNode(nodeView);
                    }
                }
            }
            return graphviewchange;
        }

        public virtual void ViewEnterZoomPositionModifier(GraphViewChange graphviewchange)
        {
            //计算中点
            
        }
        

        
        private void InitializeManipulator()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 允许拖拽Content
            this.AddManipulator(new ContentDragger());
            // 允许Selection里的内容
            this.AddManipulator(new SelectionDragger());
            // GraphView允许进行框选
            this.AddManipulator(new RectangleSelector());

            
            
            var grid = new GridBackground();
            grid.styleSheets.Add(EditorResourceLoader.Instance.BackGroundStyle);
            Insert(0, grid);
        }
        
        private void InitializeSearchMenuWindow()
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<BehaviourSearchMenuWindowProvider>();
            menuWindowProvider.OnSelectEntryHandler += OnNodeCreateTaskApply;
            menuWindowProvider.SetGraphType(CurrentGraph?.GetType());
            BehaviourSearchMenuWindowProviderInstance = menuWindowProvider;
            nodeCreationRequest += context => { SearchWindow.Open(new SearchWindowContext(context.screenMousePosition){}, menuWindowProvider); };
        }

        // 实际创建节点的回调
        private bool OnNodeCreateTaskApply(SearchTreeEntry searchtreeentry, SearchWindowContext context)
        {
            if (searchtreeentry.userData is Type targetNodeType)
            {
                CreateNewNode(targetNodeType,context.screenMousePosition);
            }
            return true;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
#if UNITY_2020_1_OR_NEWER
            Vector2 _position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            switch (evt.target)
            {
                case BehaviourGraphView:
                //默认状态
                    evt.menu.InsertAction(1, "Create Sticky Note", (e) =>
                    {
                        var data = (new BaseStickyNoteData()
                        {
                            position = new Rect(_position, new Vector2(200, 100)),
                            title = "Sticky Note"
                        });
                        CurrentGraphLayout.stickyNotes.Add(data);
                
                        AddStickyNoteView(data);
                    }, DropdownMenuAction.AlwaysEnabled);
                    //查找到delete
                    foreach (var menuItem in evt.menu.MenuItems())
                    {
                        if (menuItem is DropdownMenuAction d)
                        {
                            
                        }
                    }
                    break;
                //Node节点
                case BaseNodeView:
                    break;
                //笔记
                case BaseStickyNoteView noteView:
                    evt.menu.InsertAction(0, "Delete", (e) =>
                    {
                       RemoveStickyNoteView(noteView);
                    }, DropdownMenuAction.AlwaysEnabled);
                    break;
                
            }
#endif
        }




        public virtual void InitializeAllNode(BaseBehaviourGraph graph,BehaviourGraphLayout layout)
        {
            ClearAllNodesData();
            
            
            CurrentGraph = graph;
            _currentGraphLayout = layout;
            BehaviourSearchMenuWindowProviderInstance.SetGraphType(CurrentGraph.GetType());
            foreach (var node in CurrentGraph.nodes)
            {
                if (node is BaseBehaviourNode _node)
                {
                    GenerateSingleNode(_node);
                }
            }
            //开始连接
            foreach (var keyvalue in NodeDictionary)
            {
                foreach (var connectedNodeData in keyvalue.Value.GetAllConnectedNodes())
                {
                    if (connectedNodeData.Item3 == null || !NodeDictionary.TryGetValue(connectedNodeData.Item3, out var target)) continue;
                    if (target is null || !target.GetPort(connectedNodeData.Item2, out var targetPort)) continue;
                    if (connectedNodeData.Item1.direction == Direction.Output)
                    {
                        ConnectPorts(connectedNodeData.Item1,targetPort);
                    }
                    else
                    {
                        ConnectPorts(targetPort,connectedNodeData.Item1);
                    }
                }
            }
            foreach (var data in layout.stickyNotes)
            {
                AddStickyNoteView(data);
            }
            

        }
        public virtual void ClearAllNodesData()
        {
            //先清除前面的
            foreach (var nodeView in NodesViews)
            {
                RemoveElement(nodeView);
                
            }
            NodesViews.Clear();
            NodeDictionary.Clear();

            foreach (var edge in Edges)
            {
                RemoveElement(edge);
            }
            Edges.Clear();
            
            foreach (var stickyNoteView in StickyNoteViews)
            {
                stickyNoteView.OnSave();
                RemoveElement(stickyNoteView);
            }
            StickyNoteViews.Clear();
        }
        public void ConnectPorts(Port outputPort, Port inputPort)
        {
            // 检查是否已经连接
            if (outputPort.connected || inputPort.connected)
            {
                Debug.LogWarning("Ports are already connected!");
                return;
            }

            // 创建自定义边
            var edge = new BaseEdgeView()
            {
                output = outputPort,
                input = inputPort
            };
            // 添加边到GraphView
            AddElement(edge);
            Edges.Add(edge);
            // 连接端口
            outputPort.Connect(edge);
            inputPort.Connect(edge);
        }

        protected virtual BaseNodeView GenerateSingleNode(BaseBehaviourNode node,bool OnCreate = false)
        {
            var targetViewType = BehaviourGraphCache.Instance.GetNodeViewType(node.GetType());
            var ViewInstance = Activator.CreateInstance(targetViewType,Guid.NewGuid().ToString()) as BaseNodeView;
            //这里还需要获取到Layout的数据
            if (ViewInstance is BaseNodeView nodeView)
            {
                nodeView.Initialize(node,OnCreate);
                AddElement(nodeView);
                NodesViews.Add(nodeView);
                NodeDictionary.Add(node, nodeView);
            }

            return ViewInstance;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            // 继承的GraphView里有个Property：ports, 代表graph里所有的port
            ports.ForEach((port) =>
            {
                // 对每一个在graph里的port，进行判断，这里有两个规则：
                // 1. port不可以与自身相连
                // 2. 同一个节点的port之间不可以相连
                if (port != startPort && port.node != startPort.node)
                {
                    if (port.direction != startPort.direction)
                    {
                        compatiblePorts.Add(port);
                    }
                }
            });

            // 在我理解，这个函数就是把所有除了startNode里的port都收集起来，放到了List里
            // 所以这个函数能让StartNode的Output port与任何其他的Node的Input port相连（output port应该默认不能与output port相连吧）
            return compatiblePorts;
        }
        
        public BaseStickyNoteView AddStickyNoteView(BaseStickyNoteData data)
        {
            var c = new BaseStickyNoteView();

            c.Initialize(this,data);
            c.SetPosition(data.position);
            AddElement(c);
            StickyNoteViews.Add(c);
            return c;
        }

        public void RemoveStickyNoteView(BaseStickyNoteView view)
        {
            CurrentGraphLayout.stickyNotes.Remove(view.data);
            RemoveElement(view);
        }

        public void CreateNewNode(Type nodeType, Vector2 position)
        {
            Undo.RecordObject(CurrentGraph, "Create Node");
            var  nodeInstance =  CurrentGraph.AddNode(nodeType);
            Undo.RegisterCreatedObjectUndo(nodeInstance, "Create Node");
            if (nodeInstance.name == null || nodeInstance.name.Trim() == "") nodeInstance.name = UnityEditor.ObjectNames.NicifyVariableName(nodeType.Name);
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(CurrentGraph))) AssetDatabase.AddObjectToAsset(nodeInstance, CurrentGraph);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            NodeEditorWindow.RepaintAll();
           if (nodeInstance is BaseBehaviourNode _node)
           {
               
               var mousePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent,
                   position - _editorWindow.position.position);
               var graphMousePosition = this.contentViewContainer.WorldToLocal(mousePosition);
               
               var windowRoot = _editorWindow.rootVisualElement;
               var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, position - _editorWindow.position.position);
               var _graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
               
               nodeInstance.position = _graphMousePosition;
               var view = GenerateSingleNode(_node,true);
           }
        }

        public void DeleteNode(BaseNodeView view)
        {
            // Remove the node
            Undo.RecordObject(view.Target, "Delete Node");
            Undo.RecordObject(CurrentGraph, "Delete Node");
            foreach (var port in view.Target.Ports)
            foreach (var conn in port.GetConnections())
                Undo.RecordObject(conn.node, "Delete Node");
            CurrentGraph?.RemoveNode(view.Target);
            Undo.DestroyObjectImmediate(view.Target);
            AssetDatabase.SaveAssets();
        }
        
        public static Vector2 PositionWindowToView(BehaviourGraphView graphView, Vector2 windowPos)
        {
            var pos = windowPos - new Vector2(graphView.viewTransform.position.x, graphView.viewTransform.position.y);
            return pos / graphView.viewTransform.scale;
        }
        
    }
}