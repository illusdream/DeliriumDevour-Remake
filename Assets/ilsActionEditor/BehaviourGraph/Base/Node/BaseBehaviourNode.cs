using ilsActionEditor.Base;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using XNode;

namespace ilsActionEditor.Node
{
    [CreateNodeMenu("BaseNode")]
    public abstract class BaseBehaviourNode : XNode.Node
    {
        public BlackBoard BlackBoard
        {
            get
            {
                if (graph is BaseBehaviourGraph g)
                {
                    return g.BlackBoard;
                }

                return null;
            }
        }
    }
}