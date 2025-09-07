using ilsActionEditor.Editor.Details.NodeView;
using ilsFramework.Core;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ilsActionEditor.Editor
{
    [CustomNodeEditor(typeof(SubAFSMNode))]
    public class SubGraphNodeEditor : AFSMStateNodeView
    {
        public SubGraphNodeEditor(string guid) : base(guid)
        {
        }

        public override void GenerateAllPortFields()
        {
            base.GenerateAllPortFields();
            var target = this.Q<ObjectField>("subFSMGraph");
            target.RegisterValueChangedCallback((e) =>
            {
                //找到所有target
                var results = this.Query<SubGraphExitTranslationEditor>();
                foreach (var result in results.ToList())
                {
                    result.UpdateKeyList();
                }
            });
        }
    }
}