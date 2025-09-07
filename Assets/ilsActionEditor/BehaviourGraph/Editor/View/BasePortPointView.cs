using System;
using System.Collections.Generic;
using ilsFramework.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using XNode;
using PortView = UnityEditor.Experimental.GraphView.Port;
namespace ilsActionEditor.Editor
{
    public class BasePortPointView : PortView
    {
        /// <summary>
        /// 实际上运行时的Port
        /// </summary>
        public NodePort nodePort;
        private class CustomEdgeConnector : EdgeConnector<BaseEdgeView>
        {
            public CustomEdgeConnector(IEdgeConnectorListener listener) : base(listener) {}
        }
        
        private BasePortPointView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, System.Type type) : base(portOrientation, portDirection, portCapacity, type)
        {

        }

        public static BasePortPointView Create(Orientation orientation, Direction direction, Capacity capacity, System.Type type)
        {
            var listener = new EdgeConnectorListener();
            var port = new BasePortPointView(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new CustomEdgeConnector(listener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        public override void Connect(Edge edge)
        {

            base.Connect(edge);
        }

        public override void Select(VisualElement selectionContainer, bool additive)
        {
            base.Select(selectionContainer, additive);
        }
        
    }
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate;
        private List<GraphElement> m_EdgesToDelete;

        public EdgeConnectorListener()
        {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();
        
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }
        
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();
            
            if (edge.input.capacity == Port.Capacity.Single)
                m_EdgesToDelete.AddRange(edge.input.connections);
            if (edge.output.capacity == Port.Capacity.Single)
                m_EdgesToDelete.AddRange(edge.output.connections);
        
            if (m_EdgesToDelete.Count > 0)
                graphView.DeleteElements(m_EdgesToDelete);
        
            var edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
        
            foreach (Edge e in edgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e);
            }
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
 
        }

    }
}