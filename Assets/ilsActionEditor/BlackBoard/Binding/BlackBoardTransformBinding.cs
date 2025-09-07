using ilsFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    public class BlackBoardTransformBinding : IBlackBoardBinding
    {
        [Title("StringBinding", titleAlignment: TitleAlignments.Left)] [HideLabel] [PropertyOrder(int.MinValue)] [ShowInInspector]
        private EditorTitle _title;
        public string key;
        public Transform value;
        public void GetBinding(out string key, out object value)
        {
            key = this.key;
            value = this.value;
        }
    }
}