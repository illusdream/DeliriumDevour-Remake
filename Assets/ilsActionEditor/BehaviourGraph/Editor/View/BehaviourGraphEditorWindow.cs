using System;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ilsActionEditor.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        private BehaviourGraphView _currentView;
        
        public BehaviourGraphView CurrentView  => _currentView;
        
        private static BehaviourGraphEditorWindow _currentWindow;

        public static BehaviourGraphEditorWindow CurrentWindow
        {
            get
            {
                if (_currentWindow ==null)
                {
                    OpenWindow();
                }
                return _currentWindow;
            }
        }
        
        private BaseBehaviourGraph _currentGraph;

        public BaseBehaviourGraph CurrentGraph
        {
            get=>_currentGraph;
            private set => _currentGraph = value;
        }
        private bool _isCloneInstance = false;
        private BaseBehaviourGraph _cloneInstancePrefab;
        
        public virtual string EditorUssPath { get; set; }
        
        public virtual string EditorUmxlPath { get; set; }

        private ObjectField picker;
        private Button ApplyInstanceModify;

        private ToolbarIconButton _miniMapButton;
        private bool _showMiniMap = false;
        private MiniMap _miniMap;
        
        private ToolbarIconButton _blackBoardButton;
        private bool _showBlackBoard = false;


        [MenuItem("ilsFramework/BehaviourGraphWindow")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<BehaviourGraphEditorWindow>();
            window.titleContent = new GUIContent("BehaviourGraphWindow");
            _currentWindow = window;
        }
        
        [InitializeOnLoadMethod]
        private static void OnLoad() {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary> Handle Selection Change events</summary>
        private static void OnSelectionChanged() {
            BaseBehaviourGraph nodeGraph = Selection.activeObject as BaseBehaviourGraph;
            if (nodeGraph && !AssetDatabase.Contains(nodeGraph)) {
                //这里检测的是实时Runtime生成的副本
                //添加一些有关Editor的脚本，用来直接修改
                CurrentWindow.SetNewBehaviourGraph(nodeGraph,true);
            }
        }

        private void OnEnable()
        {
            _currentView = new BehaviourGraphView()
            {
                name = "BehaviourGraph",
                _editorWindow = this,
            };
            _currentView.StretchToParentSize();
            rootVisualElement.Add(_currentView);
          
            InitializeToolbar();
            InitializeExtensionWindow();

            Undo.undoRedoPerformed += UndoRedoPerformed;
        }
        private void UndoRedoPerformed()
        {
            if (CurrentGraph)
            {
               SetNewBehaviourGraph(CurrentGraph,_isCloneInstance);
            }
        }

        private void InitializeToolbar()
        {
            Toolbar toolbar = new Toolbar();
            
            
            OnInitializeToolbar(toolbar);
            
            ObjectField picker = new ObjectField();
            this.picker = picker;
           // picker.style.left =po
            picker.RegisterValueChangedCallback(OnChangeSelectBehaviourGraph);
            picker.allowSceneObjects = false;
            picker.objectType = typeof(BaseBehaviourGraph);
            toolbar.Add(picker);
            
            ApplyInstanceModify = new ToolbarButton();
            ApplyInstanceModify.text = "更新状态机实例";
            ApplyInstanceModify.clicked += ApplyInstanceModifyOnclicked;
            ApplyInstanceModify.SetEnabled(_isCloneInstance);
            ApplyInstanceModify.visible = false;
            toolbar.Add(ApplyInstanceModify);
            
            
            rootVisualElement.Add(toolbar);
        }
        
        public virtual void OnInitializeToolbar(Toolbar toolbar)
        {
            //添加新图的按钮
            var createNewButton = new ToolbarIconButton("", "新建Graph资产", EditorResourceLoader.Instance.CreateNewIcon);
            createNewButton.style.minWidth = 50;
            createNewButton.style.alignItems = Align.Center;
            createNewButton.style.justifyContent = Justify.Center;
            createNewButton.clicked += () =>
            {
                GenericMenu genericMenu = new GenericMenu();
                foreach (var graphType in TypeCache.GetTypesDerivedFrom<BaseBehaviourGraph>())
                {
                    
                    genericMenu.AddItem(new GUIContent($"{graphType.Name}"), false,
                        data =>
                        {
                            BaseBehaviourGraph baseGraph = BehaviourGraphCreateHelper.CreateGraph(graphType) as BaseBehaviourGraph;
                            //重新注册窗口
                           // GraphAssetCallbacks.InitializeGraph(baseGraph);
                        }, graphType);
                }
                genericMenu.ShowAsContext();
            };
            toolbar.Add(createNewButton);
            
            //开关 黑板面板的按钮，做成运行时才能显示吧
            
            //开关小地图的按钮
            var BlackBoardButton = new ToolbarIconButton("", "开启黑板", EditorResourceLoader.Instance.BlackBoardIcon);
            BlackBoardButton.style.minWidth = 50;
            BlackBoardButton.style.alignItems = Align.Center;
            BlackBoardButton.style.justifyContent = Justify.Center;
            BlackBoardButton.clicked += () =>
            {
                _showBlackBoard = !_showBlackBoard;
            };
            _blackBoardButton = BlackBoardButton;
            toolbar.Add(BlackBoardButton);
            
            //开关小地图的按钮
            var mipmapButton = new ToolbarIconButton("", "开启小地图", EditorResourceLoader.Instance.ToggleMipMapIcon);
            mipmapButton.style.minWidth = 50;
            mipmapButton.style.alignItems = Align.Center;
            mipmapButton.style.justifyContent = Justify.Center;
            mipmapButton.clicked += () =>
            {
                _showMiniMap = !_showMiniMap;
                _miniMap.visible = _showMiniMap;
            };
            _miniMapButton = mipmapButton;
            //mipmapButton.
            toolbar.Add(mipmapButton);
            
            //定位至资产文件的按钮
            var pingToAssetButton = new ToolbarIconButton("","定位至资产",EditorResourceLoader.Instance.PingToAssetIcon);
            pingToAssetButton.style.minWidth = 50;
            pingToAssetButton.style.alignItems = Align.Center;
            pingToAssetButton.style.justifyContent = Justify.Center;
            pingToAssetButton.clicked += () =>
            {
                if (EditorApplication.isPlaying)
                {
                    if (_isCloneInstance)
                    {
                        if (_cloneInstancePrefab)
                        {
                            EditorGUIUtility.PingObject(_cloneInstancePrefab);
                            Selection.activeObject = _cloneInstancePrefab;
                        }
                    }
                    else
                    {
                        if (CurrentGraph)
                        {
                            EditorGUIUtility.PingObject(CurrentGraph);
                            Selection.activeObject = CurrentGraph;
                        }
                    }
                }
                else
                {
                    //这个时候肯定是资产
                    if (CurrentGraph)
                    {
                        EditorGUIUtility.PingObject(CurrentGraph);
                        Selection.activeObject = CurrentGraph;
                    }
                }
            };
            
            toolbar.Add(pingToAssetButton);
        }
        
        private void InitializeExtensionWindow()
        {
            _miniMap = new MiniMap(){anchored = true};
            _miniMap.visible = false;
            CurrentView.Add(_miniMap);
        }

        private void ApplyInstanceModifyOnclicked()
        {
            2222.LogSelf();
        }

        private void OnChangeSelectBehaviourGraph(ChangeEvent<Object> evt)
        {
            if (evt.newValue is BaseBehaviourGraph newGraph)
            {
                SetNewBehaviourGraph(newGraph);
            }

            if (evt.newValue == null)
            {
                CurrentView.ClearAllNodesData();
            }
        }

        public void SetNewBehaviourGraph(BaseBehaviourGraph newGraph,bool IsInstanceClone = false)
        {
            picker.SetValueWithoutNotify(newGraph);
            if (IsInstanceClone)
            {
                _cloneInstancePrefab = newGraph.CopyFrom;
            }
            _isCloneInstance = IsInstanceClone;
            ApplyInstanceModify.visible = _isCloneInstance;
            ApplyInstanceModify.SetEnabled(_isCloneInstance);
            ApplySelectedBehaviourGraph(CurrentGraph,newGraph);
        }
        
        private void ApplySelectedBehaviourGraph(BaseBehaviourGraph oldGraph, BaseBehaviourGraph newGraph)
        {
            if (oldGraph)
            {
                
            }

            if (newGraph)
            {
                //查找layout
                string layoutGUIDName = "";
                if (_isCloneInstance)
                {
                    layoutGUIDName = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_cloneInstancePrefab));
                }
                else
                {
                    layoutGUIDName = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newGraph));
                }

                var layoutPath = $"Assets/ilsActionEditor/BehaviourGraph/Editor/View/Save/{layoutGUIDName}.asset";
                var layoutInstance = AssetDatabase.LoadAssetAtPath<BehaviourGraphLayout>(layoutPath);
                if (!layoutInstance)
                {
                    //添加布局
                    layoutInstance = ScriptableObject.CreateInstance<BehaviourGraphLayout>();
                    AssetDatabase.CreateAsset(layoutInstance, layoutPath);
                }
                CurrentView.InitializeAllNode(newGraph,layoutInstance);
            }
            else
            {
                111.LogSelf();
                CurrentView.ClearAllNodesData();
            }
            CurrentGraph = newGraph;
        }

        public void Update()
        {
            if (_isCloneInstance && EditorApplication.isPlaying == false)
            {
                picker.SetValueWithoutNotify(_cloneInstancePrefab);
                SetNewBehaviourGraph(_cloneInstancePrefab,false);
            }

            picker.allowSceneObjects = EditorApplication.isPlaying;
            

            _miniMapButton.tooltip = _showMiniMap ? "关闭小地图" : "开启小地图";
            _miniMap?.SetPosition(new Rect(this.position.size.x - 205, this.position.size.y - 205, 200, 200));
            
            
            _blackBoardButton.tooltip = _showMiniMap ? "关闭黑板" : "开启黑板";
            _blackBoardButton.SetEnabled((CurrentGraph?.BlackBoard != null));
        }
        

        private void OnDisable()
        {
            rootVisualElement.Remove(_currentView);
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }
        
        
    }
}