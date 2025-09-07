using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codice.Client.BaseCommands;
using ilsActionEditor.Base;
using ilsActionEditor.Node;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using XNode;
using Object = System.Object;

namespace ilsActionEditor.Editor.Details.NodeView
{
    [CustomNodeEditor(typeof(AFSMStateNode))]
    public class AFSMStateNodeView : BaseNodeView<AFSMStateNode>
    {
        public static readonly Color DefaultTitleColor = new Color(0.212f, 0.212f, 0.212f, 1.000f);
        
        public Color currentDefaultTitleColor = DefaultTitleColor;
        
        public bool CanSetAsEntryState = true;
        
        public BiMap<AFSMStateTranslation,BasePortView> Ports = new BiMap<AFSMStateTranslation,BasePortView>();
        
        public AFSMStateNodeView(string guid) : base(guid)
        {
        }

        ~AFSMStateNodeView()
        {

        }
        
        private Dictionary<FieldInfo,VisualElement> fieldsInGraph = new Dictionary<FieldInfo,VisualElement>();
    
        
        
        public override void OnStartInitialize(BaseBehaviourNode node)
        {
            var color = new Color(54 / 255f, 54 / 255f, 54 / 255f);
            NodeContentContainer.style.backgroundColor = color;
            mainContainer.style.backgroundColor = color;
            titleContainer.style.backgroundColor = color;
            inputContainer.style.backgroundColor = color;
            outputContainer.style.backgroundColor = color;
            
            topContainer.style.justifyContent = Justify.SpaceBetween;
            outputContainer.style.width = Length.Percent(50f);
            inputContainer.style.width = Length.Percent(50f);
            
            m_CollapseButton.visible = false;
            m_CollapseButton.SetEnabled(false);

            if (TargetNode.GetType().GetCustomAttributes(typeof(NodeColorAttribute),true).FirstOrDefault() is NodeColorAttribute colorAttr)
            {
                currentDefaultTitleColor = colorAttr.NodeColor;
            }

            if (TargetNode.GetType().IsDefined(typeof(NonEntryStateAttribute)))
            {
                CanSetAsEntryState = false;
            }

            InitAddEntryTranslationButton();
            InitAddExitTranslationButton();
            
            GenerateAllPortFields();

            schedule.Execute(UpdatePerFrame).Every(10);
            schedule.Execute(UpdatePerMin).Every(1000);


            base.OnStartInitialize(node);
        }
        
        public virtual void GenerateAllPortFields()
        {
            DefaultGenerateAllPortFields(); 
        }

        public void DefaultGenerateAllPortFields()
        {
            var fields = TargetNode.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                //只关注在AFSMNode后生成的
                .Where(f => f.DeclaringType != typeof(AFSMNode) 
                            && (f.CustomAttributes.Count(attr => attr.AttributeType == typeof(ShowInGraphAttribute)) >0));
            foreach (var field in fields)
            {
                var view = CreateFieldsView(field,ObjectNames.NicifyVariableName(field.Name));
            }
        }

        public VisualElement CreateFieldsView(FieldInfo field, string label = null, Action valueChangedCallback = null)
        {
            if(field == null)
                return null;

            var view = FieldFactory.CreateField(field.FieldType, field.GetValue(TargetNode), (newValue) =>
            {
                field.SetValue(TargetNode, newValue);
                valueChangedCallback?.Invoke();
            }, label);

            if (view != null)
            {
                view.name = field.Name;
                NodeContentContainer.Add(view);
                fieldsInGraph.Add(field,view);
            }

            return view;
        }
        
        private void InitAddEntryTranslationButton()
        {
            var button = new ToolbarIconButton("AddEntryTranslation",null,null)
            {
            };
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            var menuWindowProvider = ScriptableObject.CreateInstance<AFSMStateNodeSearchMenuProvider>();
            menuWindowProvider.OnSelectEntryHandler += OnSelectEntryHandler;
            menuWindowProvider.TargetPorts = TargetNode.GetAllCanUseEntryTransitions();
            button.clicked += () =>
            {
                var parameterType = new GenericMenu();

                foreach (var paramType in TargetNode.GetAllCanUseEntryTransitions())
                    parameterType.AddItem(new GUIContent(paramType.Name), false, () =>
                    {
                        var t= TargetNode.AddEntryTransition(paramType,TargetNode);
                        AddInputPort(t);
                    });
                parameterType.ShowAsContext();
            };
            inputContainer.Insert(0,button);
            
        }
        private void InitAddExitTranslationButton()
        {
            var button = new ToolbarIconButton("AddExitTranslation",null,null)
            {
            };
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            var menuWindowProvider = ScriptableObject.CreateInstance<AFSMStateNodeSearchMenuProvider>();
            menuWindowProvider.OnSelectEntryHandler += OnSelectEntryHandler;
            menuWindowProvider.TargetPorts = TargetNode.GetAllCanUseEntryTransitions();
            button.clicked += () =>
            {
                var parameterType = new GenericMenu();

                foreach (var paramType in TargetNode.GetAllCanUseExitTransitions())
                    parameterType.AddItem(new GUIContent(paramType.Name), false, () =>
                    {
                        var t= TargetNode.AddExitTransition(paramType,TargetNode);
                        AddOutputPort(t);
                    });
                parameterType.ShowAsContext();
            };
            outputContainer.Insert(0,button);
            
        }
        private bool OnSelectEntryHandler(SearchTreeEntry searchtreeentry, SearchWindowContext context)
        {
            return false;
        }

        public override void InitializeAllPort(BaseBehaviourNode node)
        {
            foreach (var entryTranslation in TargetNode.fieldEntryTranslations)
            {
                var pv = AddInputPort(entryTranslation);
                pv.DontModifier = true;
            }
            foreach (var entryTranslation in TargetNode.Entries)
            {
                AddInputPort(entryTranslation);
            }
            foreach (var exitTranslation in TargetNode.fieldExitTranslations)
            {
                var pv =  AddOutputPort(exitTranslation);
                pv.DontModifier = true;
            }
            foreach (var exitTranslation in TargetNode.Exits)
            {
                AddOutputPort(exitTranslation);
            }
            base.InitializeAllPort(node);
        }

        public BasePortView AddInputPort(AFSMStateTranslation translation)
        {
            var p= InstantiatePortView(translation.GetPort(),Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(AFSMStateTranslation));
            p.portName = translation.PortName;
            var pv = new ilsActionEditor.Editor.BasePortView(translation,p,this);
            pv.PortContentContainer.style.alignItems = Align.Stretch;
            pv.PortPointContainer.Add(p);
            inputContainer.Add(CreateDivider());
            inputContainer.Add(pv);
            Ports.Set(translation,pv);
            
            return pv;
        }
        public BasePortView AddOutputPort(AFSMStateTranslation translation)
        {
            var p=InstantiatePortView(translation.GetPort(),Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(AFSMStateTranslation));
            p.portName = translation.PortName;
            var pv = GeneratePortView(translation,p);
            pv.PortContentContainer.style.alignItems = Align.Stretch;
            pv.PortPointContainer.Add(p);

            outputContainer.Add(CreateDivider());
            outputContainer.Add(pv);
            Ports.Set(translation,pv);
            return pv;
        }
        
        protected virtual BasePortView GeneratePortView(AFSMStateTranslation translation,BasePortPointView pointView,bool OnCreate = false)
        {
            var targetViewType = BehaviourGraphCache.Instance.GetPortViewType(translation.GetType());
            var ViewInstance = Activator.CreateInstance(targetViewType,new Object[]{translation,pointView,this}) as BasePortView;
            return ViewInstance;
        }
        private VisualElement CreateDivider()
        {
            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            divider.style.marginTop = 5;   // 分割线上方的间隔
            divider.style.marginBottom = 5; // 分割线下方的间隔
            return divider;
        }
        public override void BeforeInitializeView()
        {
            CheckFieldTranslation();
            base.BeforeInitializeView();
        }

        //应该要在create部分开始
        private void CheckFieldTranslation()
        {
            CheckEntryTranslation();
            CheckExitTranslation();
            void CheckEntryTranslation()
            {
                var type = TargetNode.GetType();
                var allFieldWithTranslationAttribute = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(t =>
                    t.IsDefined(typeof(EntryTranslationAttribute), true) && typeof(BaseEntryTranslation).IsAssignableFrom(t.FieldType));
                foreach (var fieldInfo in allFieldWithTranslationAttribute)
                {
                    var instance = (AFSMStateTranslation)fieldInfo.GetValue(TargetNode);
                    if (instance is null)
                    {
                        instance = Activator.CreateInstance(fieldInfo.FieldType,TargetNode,fieldInfo.Name) as AFSMStateTranslation;
                        instance.Name = fieldInfo.Name;
                        instance.PortName = fieldInfo.Name;
                        instance.IO = NodePort.IO.Input;
                        instance.Node = TargetNode;
                        TargetNode.fieldEntryTranslations.Add(instance);
                    }
        
                    if (instance.Name == null || instance.Name == "")
                    {
                        instance.Name = fieldInfo.Name;
                        instance.PortName = fieldInfo.Name;
                        instance.IO = NodePort.IO.Input;
                        instance.Node = TargetNode;
                    }

                    TargetNode.fieldEntryTranslations.Add(instance);
                    var port = TargetNode.GetInputPort("entry" + instance.Name);
                    //port.AddConnections();
                    if (port == null)
                    {
                        TargetNode.AddDynamicInput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
                            "entry" + instance.Name);
                    }
                }
            }

            void CheckExitTranslation()
            {
                //清除原始数据
                var type = TargetNode.GetType();
                var allFieldWithTranslationAttribute = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(t =>
                        t.IsDefined(typeof(ExitTranslationAttribute), true) && typeof(BaseExitTranslation).IsAssignableFrom(t.FieldType));
                foreach (var fieldInfo in allFieldWithTranslationAttribute)
                {
                    var instance = (AFSMStateTranslation)fieldInfo.GetValue(TargetNode);
                    if (instance is null)
                    {
                        instance = Activator.CreateInstance(fieldInfo.FieldType, TargetNode, fieldInfo.Name) as AFSMStateTranslation;
                        instance.Name = fieldInfo.Name;
                        instance.PortName = fieldInfo.Name;
                        instance.IO = NodePort.IO.Output;
                        instance.Node = TargetNode;
                        fieldInfo.SetValue(TargetNode, instance);
                    }

                    if (instance.Name == null || instance.Name == "")
                    {
                        instance.Name = fieldInfo.Name;
                        instance.PortName = fieldInfo.Name;
                        instance.IO = NodePort.IO.Output;
                        instance.Node = TargetNode;
                    }

                    TargetNode.fieldExitTranslations.Add(instance);
                    var port = TargetNode.GetOutputPort("exit" + instance.Name);
                    if (port == null)
                    {
                        TargetNode.AddDynamicOutput(typeof(AFSMStateTranslation), XNode.Node.ConnectionType.Override, XNode.Node.TypeConstraint.Inherited,
                            "exit" + instance.Name);
                    }
                }
            }
        }
        
        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return base.InstantiatePort(orientation, direction, capacity, type);
        }

        public virtual BasePortPointView InstantiatePortView(NodePort port,Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            var view = BasePortPointView.Create(orientation,direction,capacity,type);
            view.nodePort = port;
            return view;
        }

        public override List<(BasePortPointView,BaseConnection, BaseBehaviourNode)> GetAllConnectedNodes()
        {
            return Ports.Where((pair)=>pair.Value.view.direction == Direction.Output)
                .Select((pair =>  (pair.Value.view,pair.Key.GetNextStateNodeEntryTranslation<AFSMStateTranslation>() as BaseConnection,pair.Key.TargetNextState() as BaseBehaviourNode))).ToList();
        }

        public override bool GetPort(BaseConnection connection, out BasePortPointView portPoint)
        {
            if (connection is AFSMStateTranslation t)
            {
                var result =Ports.TryGetRight(t, out var _portPoint);
                portPoint = _portPoint.view;
                return result;
            }
            return base.GetPort(connection, out portPoint);
        }

        public override void RemovePort(BasePortView portPoint)
        {
            if (Ports.TryGetLeft(portPoint, out var translation))
            {
                switch (portPoint.view.direction)
                {
                    case Direction.Input:
                    {
                        var index = inputContainer.IndexOf(portPoint);
                        inputContainer.RemoveAt(index -1);
                        inputContainer.RemoveAt(index -1);
                        TargetNode.RemoveEntryTransition(translation.Name);
                    }
                        break;
                    case Direction.Output:
                    {
                        var index = outputContainer.IndexOf(portPoint);
                        outputContainer.RemoveAt(index -1);
                        outputContainer.RemoveAt(index -1);
                        TargetNode.RemoveExitTransition(translation.Name);
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            base.RemovePort(portPoint);
        }

        public override void RenamePort(BasePortView portView)
        {
            if (Ports.TryGetLeft(portView, out var translation))
            {
                TranslationRenameWindow.Show(translation,(finalName)=> portView.view.portName = finalName);
            }
 
            base.RenamePort(portView);
        }

        public virtual void UpdatePerMin()
        {
            UpdateAllFieldInGraph();
        }

        public virtual void UpdatePerFrame()
        {
            UpdateTitleColor();   
        }
        
        
        public void UpdateAllFieldInGraph()
        {
            foreach (var field in fieldsInGraph)
            {
                var result = field.Value.GetType().GetMethod("SetValueWithoutNotify");
                if (result != null)
                {
                    result.Invoke(field.Value, new []{field.Key.GetValue(TargetNode)});
                }
            }
        }

        public virtual void UpdateTitleColor()
        {
            if (EditorApplication.isPlaying)
            {
                if (TargetNode.FSM.CurrentState == TargetNode)
                {
                    SetTitleColor(new Color(Color.yellow.r * 0.4f,Color.yellow.g * 0.4f,Color.yellow.b * 0.4f,1));
                    return;
                }
            }

            if (TargetNode.FSM.EntryState == TargetNode)
            {
                SetTitleColor(new Color(0,0.4f,0,1));
            }
            else
            {
                SetTitleColor();
            }
        }

        public void SetTitleColor(Color? color = null)
        {
            var finalValue = color.GetValueOrDefault(currentDefaultTitleColor);
            if (!finalValue.Equals( titleContainer.style.backgroundColor.value))
            {
                titleContainer.style.backgroundColor = finalValue;
            }

        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (CanSetAsEntryState)
            {
                evt.menu.AppendAction("Set As EntryState", new Action<DropdownMenuAction>((t) =>
                {
                    TargetNode.FSM.SetEntryState(TargetNode);
                }), new Func<DropdownMenuAction, DropdownMenuAction.Status>((t)=> DropdownMenuAction.Status.Normal));
                evt.menu.AppendSeparator();
            }
            base.BuildContextualMenu(evt);
        }
        
        
    }
}