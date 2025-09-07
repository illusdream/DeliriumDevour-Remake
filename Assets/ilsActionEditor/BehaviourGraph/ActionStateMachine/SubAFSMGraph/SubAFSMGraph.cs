using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ilsActionEditor
{
    [CreateAssetMenu(fileName = "SubAFSMGraph", menuName = "ilsActionEditor/SubFSMGraph")]
    //[GraphCategory("SubAFSMGraph")]
    public class SubAFSMGraph : AFSMGraph
    {
        public static string NULLExitStringKey = "NULL";

        public void OnValidate()
        {

        }

        public virtual bool IsInExitNode()
        {
            return CurrentState is SubAFSMExitNode;
        }

        /// <summary>
        /// 当前要退出的ExitNode的key
        /// </summary>
        /// <returns></returns>
        public virtual string CurrentExitKey()
        {
            if (CurrentState is SubAFSMExitNode exitNode)
            {
                return exitNode.ExitKey;
            }
            return NULLExitStringKey;
        }

#if UNITY_EDITOR
        public virtual HashSet<string> GetAllExitNodeKeys()
        {
            HashSet<string> keys = new HashSet<string>();
            int counter = 0;
            foreach (var node in nodes)
            {
                if (node is SubAFSMExitNode exitNode)
                {
                    counter++;
                    keys.Add(exitNode.ExitKey);
                }
            }

            if (counter == 0)
            {
                keys.Add(NULLExitStringKey);
            }
            return keys;
        }
#endif
    }
}