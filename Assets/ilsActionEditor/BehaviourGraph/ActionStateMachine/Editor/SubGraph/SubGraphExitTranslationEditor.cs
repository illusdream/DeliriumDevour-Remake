using System.Linq;
using ilsFramework.Core;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace ilsActionEditor.Editor
{
    [CustomPortEditor(typeof(SubAFSMNode.SubGraphDefaultTranslation))]
    public class SubGraphExitTranslationEditor : BasePortView
    {
        DropdownField ExitDropdownVisualElement;
        
        public SubGraphExitTranslationEditor(AFSMStateTranslation translation, BasePortPointView portPointView, BaseNodeView nodeView) : base(translation, portPointView, nodeView)
        {
        }

        public override void GenerateAllPortFields()
        {
            if (Translation is SubAFSMNode.SubGraphDefaultTranslation targetTranslation && targetTranslation.Node is SubAFSMNode subNode)
            {
                var currentKeyValue = targetTranslation.ExitKey;
                var list = subNode.GetAllExitNodeKeys();
                if (!list.Contains(currentKeyValue))
                {
                    currentKeyValue = list.First();
                }
                var _enum = new DropdownField("Exit Key",list.ToList(),currentKeyValue);
                _enum.RegisterValueChangedCallback(ExitKeyChangedCallBack);
                ExitDropdownVisualElement = _enum;
                PortContentContainer.Insert(0, _enum);
            }
            base.GenerateAllPortFields();
        }

        private void ExitKeyChangedCallBack(ChangeEvent<string> changed)
        {
            if (Translation is SubAFSMNode.SubGraphDefaultTranslation targetTranslation)
            {
                targetTranslation.ExitKey = changed.newValue;
            }
        }

        public void UpdateKeyList()
        {
            if (Translation is SubAFSMNode.SubGraphDefaultTranslation targetTranslation && targetTranslation.Node is SubAFSMNode subNode)
            {
                var currentKeyValue = targetTranslation.ExitKey;
                var list = subNode.GetAllExitNodeKeys();
                if (!list.Contains(currentKeyValue))
                {
                    currentKeyValue = list.First();
                }
                ExitDropdownVisualElement.value = currentKeyValue;
                ExitDropdownVisualElement.choices = list.ToList();
            }
        }
    }
}