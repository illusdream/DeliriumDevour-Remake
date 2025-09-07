using System;
using ilsFramework;
using Sirenix.OdinInspector;

namespace ilsActionEditor
{
    [Serializable]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public class BlackBoardStringBinding : IBlackBoardBinding
    {
        [Title("StringBinding", titleAlignment: TitleAlignments.Left)] [HideLabel] [PropertyOrder(int.MinValue)] [ShowInInspector]
        private EditorTitle _title;
        public string key;
        public string value;
        public void GetBinding(out string key, out object value)
        {
            key = this.key;
            value = this.value;
        }
    }
}