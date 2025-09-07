using System.Linq;
using ilsActionEditor.Editor.Details.NodeView;
using ilsActionEditor.Node;
using UnityEngine.UIElements;

namespace ilsActionEditor.Editor
{
    [CustomNodeEditor(typeof(SubAFSMExitNode))]
    public class SubGraphExitNodeView : AFSMStateNodeView
    {
        public SubGraphExitNodeView(string guid) : base(guid)
        {
        }

        public override void InitializeAllPort(BaseBehaviourNode node)
        {
            base.InitializeAllPort(node);
        }

        public override void GenerateAllPortFields()
        {
            base.GenerateAllPortFields();
            var target = this.Q<TextField>("ExitKey");
            if (TargetNode is SubAFSMExitNode node)
            {
                if (node.ExitKey == null)
                {
                    var count = node.graph.nodes.Count((e) => e.GetType() == typeof(SubAFSMExitNode));
                    node.ExitKey = (count).ToString();
                    target.SetValueWithoutNotify( node.ExitKey);
                }
            }
        }
    }
}