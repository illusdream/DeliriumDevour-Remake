 using System.Collections.Generic;
 using System.Linq;
 using System.Runtime.InteropServices;
 using ilsActionEditor.Base;
 using ilsActionEditor.Editor;
 using ilsActionEditor.Node;
 using ilsFramework.Core;
 using Sirenix.OdinInspector;
 using UnityEditor;
 using UnityEditor.Experimental.GraphView;
 using UnityEditor.UIElements;
 using UnityEngine;
 using UnityEngine.UIElements;
 using NodeView = UnityEditor.Experimental.GraphView.Node;
 using Object = System.Object;

 namespace ilsActionEditor
{
    public class BaseNodeView : NodeView
    {
        public string GUID;

        private BaseBehaviourNode _target;
        public BaseBehaviourNode Target=>_target;

        private bool _isCloneInstance;
        public bool IsCloneInstance=>_isCloneInstance;

        private BaseBehaviourNode _prefabTarget;

        public BaseBehaviourNode PrefabTarget
        {
            get
            {
                if (IsCloneInstance)
                {
                    return _prefabTarget;
                }
                return null;
            }
        }

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        
        public virtual float Width => 350f;
        
        public VisualElement NodeContentContainer { get; private set; }
        
        
        public BaseNodeView(string guid)
        {
            this.GUID = guid;
        }

        public void Initialize(BaseBehaviourNode node,bool OnCreate = false)
        {
            _isInitialized = true;
            _target = node;
            title = (node?.name);
            style.minWidth = Width;
            style.maxWidth = Width;
            //遍历查找
            var currentType = _target.GetType();
            while (currentType != typeof(BaseBehaviourNode) && currentType != typeof(Object) && currentType != null)
            {
                if ((currentType.IsDefined(typeof(NodeWidthAttribute),false)))
                {
                    var attrs = currentType.GetCustomAttributes(typeof(NodeWidthAttribute),false);
                    if (attrs.Length > 0)
                    {
                        var width = attrs.OfType<NodeWidthAttribute>().FirstOrDefault();
                        if (width != null)
                        {
                            
                            style.minWidth = width.NodeWidth;
                            style.maxWidth = width.NodeWidth;
                            break;
                        }
                    }
                }
                currentType = currentType?.BaseType;
            }

            SetPosition(new Rect(node.position,Vector2.one *100));
            InitializeAllContainer();
            if (OnCreate)
            {
                BeforeInitializeView();
            }
            OnStartInitialize(Target);
            InitializeAllPort(Target);
            OnEndInitialize(Target);
            _isInitialized = false;
        }

        private void InitializeAllContainer()
        {
            NodeContentContainer = new VisualElement() {name = "NodeContent"};
            NodeContentContainer.AddToClassList("NodeControls");
            mainContainer.Insert(1,NodeContentContainer);
            
    
        }
        
        public virtual void OnStartInitialize(BaseBehaviourNode node)
        {
            
        }

        public virtual void InitializeAllPort(BaseBehaviourNode node)
        {
            //留给后人
        }

        public virtual void OnEndInitialize(BaseBehaviourNode node)
        {
            
        }

        public void SetCloneSets(bool isCloneInstance, BaseBehaviourNode prefabTarget)
        {
            
        }
        
        public override void SetPosition(Rect newPos)
        {
            OnSetPosition(newPos,IsInitialized);
            base.SetPosition(newPos);
        }

        public virtual void OnSetPosition(Rect newPos,bool isInitialized)
        {
            if (!isInitialized)
            {
                Target.position = newPos.position;
            }
        }
        //暂时先不做这些吧，图状态只能再非Playing状态下修改，为的是防止最后修改不同步的问题发生
        //这不就是正常的MonoBehaviour逻辑吗....
        public void ApplyModifiedProperties()
        {
            
        }

        public virtual List<(BasePortPointView,BaseConnection,BaseBehaviourNode)> GetAllConnectedNodes()
        {
            return new List<(BasePortPointView,BaseConnection, BaseBehaviourNode)>();
        }

        public virtual bool GetPort(BaseConnection connection, out BasePortPointView portPoint)
        {
            portPoint = null;
            return false;
        }

        public override void Select(VisualElement selectionContainer, bool additive)
        {
            Selection.activeObject = _target;
            base.Select(selectionContainer, additive);
        }
        
        

        public virtual void BeforeInitializeView()
        {
            
        }

        public virtual void RemovePort(BasePortView portView)
        {
            
        }

        public virtual void RenamePort(BasePortView portView)
        {
            
        }
    }

    public class BaseNodeView<T> : BaseNodeView where T : BaseBehaviourNode
    {
        public BaseNodeView(string guid) : base(guid)
        {
        }
        public T TargetNode => (T)Target;
        
        
    }
}