using System;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace ilsActionEditor.Base
{
    [Serializable]
    public class BaseConnection
    {
        [SerializeField]
        [HideInInspector]
        private XNode.Node _node;

        [SerializeField]
        [HideInInspector]
        private string _portName;
       [HideInInspector]
        public NodePort.IO IO;
        
        public XNode.Node Node
        {
            get=> _node;
            set=>_node=value;
        }

        public string PortName
        {
            get=> _portName;
            set => _portName=value;
        }
        public BaseConnection(XNode.Node node, string portName)
        {
            _node = node;
            _portName = portName;
        }

        public virtual NodePort GetPort()
        {
            return _node.GetPort(_portName);
        }
        
        
    }
}