using System.Linq;
using ilsActionEditor.BehaviourGraph;
using ilsFramework.Core;

namespace ilsActionEditor.Editor
{
    [CustomNodeGraphEditor(typeof(AFSMGraph))]
    public class AFSMEditor : BehaviourGraphEditor
    {
        public override void OnOpen()
        {
           // UpdateAllNodeWithTranlationAttribute();
        }

        public override void OnCreate()
        {
           // UpdateAllNodeWithTranlationAttribute();
            base.OnCreate();
        }

        public override void OnWindowFocus()
        {
            base.OnWindowFocus();
        }

     

        public override void OnClose()
        {

        }
    }
}