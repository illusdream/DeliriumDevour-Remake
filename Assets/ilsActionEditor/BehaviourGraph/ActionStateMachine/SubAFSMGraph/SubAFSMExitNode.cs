using UnityEngine;

namespace ilsActionEditor
{
    [NodeTint(1f,0f,0f)]
    [NodeColor(1f,0f,0f,1f)]
    [NonEntryState]
    [NodeMenuItem("SubAFSMGraph/ExitNode")]
    public class SubAFSMExitNode : AFSMStateNode
    {
        [ShowInGraph]
        public string ExitKey;
    }
}