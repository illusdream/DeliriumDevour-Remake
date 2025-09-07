using System;
using ilsActionEditor.Node;
using ilsFramework.Core;
using UnityEngine;
using XNode;

namespace ilsActionEditor
{
    [NodeMenuItem("Test/AFSMNode")]
    public abstract class AFSMNode : BaseBehaviourNode
    {
        [SerializeField]
        private string _name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return this.name;
                }
                return _name;
            }
        }
        public AFSMGraph FSM
        {
            get=> graph as AFSMGraph;
        }

        public override object GetValue(NodePort port)
        {
            return base.GetValue(port);
            return null;
        }


        public T GetInputValue<T>(string portName)
        {
            NodePort port = GetInputPort(portName);
            return (T)GetValue(port);
        }
    }
}