using System;
using ilsActionEditor.Editor;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ilsActionEditor
{
    public class BehaviourGraphAssetCallBack
    {
        [OnOpenAsset(-Int32.MaxValue)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var baseGraph = EditorUtility.InstanceIDToObject(instanceID) as BaseBehaviourGraph;
            if (baseGraph != null)
            {
                BehaviourGraphEditorWindow.CurrentWindow.SetNewBehaviourGraph(baseGraph);
                return true;
            }

            return false;
        }
    }
}