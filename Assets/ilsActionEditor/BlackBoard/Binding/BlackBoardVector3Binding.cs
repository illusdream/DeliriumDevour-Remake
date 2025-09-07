using System;
using ilsFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    [Serializable]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public class BlackBoardVector3Binding : IBlackBoardBinding
    {
        [Title("Vector3Binding", titleAlignment: TitleAlignments.Left)] [HideLabel] [PropertyOrder(int.MinValue)] [ShowInInspector]
        private EditorTitle _title;
        public string key;
        public Vector3 value;


        public void GetBinding(out string key, out object value)
        {
            key = this.key;
            value = this.value;
        }
    }
}