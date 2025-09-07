using System.Collections.Generic;
using UnityEngine;

namespace ilsActionEditor.Editor
{
    /// <summary>
    /// 这个用来保存图的布局信息
    /// 包括 便签，group ，relay节点这些的
    /// </summary>
    public class BehaviourGraphLayout : ScriptableObject
    {
        public List<BaseStickyNoteData> stickyNotes = new List<BaseStickyNoteData>();
    }
}