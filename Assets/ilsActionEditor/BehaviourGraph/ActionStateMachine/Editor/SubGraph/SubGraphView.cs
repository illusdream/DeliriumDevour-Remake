using ilsFramework.Core;

namespace ilsActionEditor.Editor
{
    public class SubGraphView : BehaviourGraphView
    {
        public override void InitializeAllNode(BaseBehaviourGraph graph, BehaviourGraphLayout layout)
        {
            111.LogSelf();
            base.InitializeAllNode(graph, layout);
        }
    }
}