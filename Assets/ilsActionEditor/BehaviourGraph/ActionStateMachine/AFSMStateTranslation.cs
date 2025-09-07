using System;
using ilsActionEditor.Base;
using ilsFramework.Core;
using UnityEngine;
using XNode;

namespace ilsActionEditor
{
    [Serializable]
    public abstract class AFSMStateTranslation : BaseConnection
    {
        public AFSMStateTranslation(XNode.Node node, string portName) : base(node, portName)
        {
            _name = portName;
        }
        [SerializeField]
        [HideInInspector]
        private string _name ="";
        
        public string Name
        {
            get { return _name; }
            set
            {
                PortName = value;
                _name = value;
            }
        }

        public void Rename(string newName)
        {
            var port = GetPort();
            var otherPort = GetConnection();
            switch (IO)
            {
                case NodePort.IO.Input:
                    Node.RemoveDynamicPort("entry" + PortName);
                    port.RenameFieldName("entry" + newName);
  
                    Node.AddDynamicPort(port);
                    port.Connect(otherPort);
                    break;
                case NodePort.IO.Output:
                    Node.RemoveDynamicPort("exit" + PortName);
                    port.RenameFieldName("exit" + newName);
                    Node.AddDynamicPort(port);
                    port.Connect(otherPort);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Name = newName;
        }

        public AFSMStateNode State
        {
            get => Node as AFSMStateNode;
        }

        public BlackBoard BlackBoard => State.BlackBoard;

        
        public void Initialize(AFSMStateNode currentState)
        {
            Node = currentState;
            OnInitialize();
        }
        public virtual void OnInitialize() { }
        /// <summary>
        /// 检测是否可以转换状态
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTranslate()
        {
            return true;
        }

        public virtual AFSMStateNode TargetNextState()
        {
            return GetPort().Connection?.node as AFSMStateNode;
        }

        public virtual T GetNextStateNodeEntryTranslation<T>() where T : AFSMStateTranslation
        {
            return TargetNextState()?.GetInputValue<T>(GetConnection().fieldName);
        }
        public override NodePort GetPort()
        {
            return IO switch
            {
                NodePort.IO.Input => Node.GetPort("entry" + PortName),
                NodePort.IO.Output => Node.GetPort("exit" + PortName)
            };
        }

        public virtual NodePort GetConnection()
        {
            var port = GetPort();
            return port.Connection;
        }
        
        public virtual void StateEnter(){}
        public virtual void Update(float deltaTime){}
        public virtual void StateExit(){}
    }

    public class BaseEntryTranslation : AFSMStateTranslation
    {
        public BaseEntryTranslation(XNode.Node node, string portName) : base(node, portName)
        {
        }
    }
    
    public class BaseExitTranslation : AFSMStateTranslation
    {
        public BaseExitTranslation(XNode.Node node, string portName) : base(node, portName)
        {
        }
    }
}