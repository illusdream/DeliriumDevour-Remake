using System;
using System.Collections.Generic;
using Codice.Client.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ilsActionEditor.Editor.Details.NodeView
{
    public class AFSMStateNodeSearchMenuProvider : ScriptableObject,ISearchWindowProvider
    {
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var final = new List<SearchTreeEntry>();
            Dictionary<string,SearchTreeEntry> searchTree = new Dictionary<string,SearchTreeEntry>();
            var title = new SearchTreeGroupEntry(new GUIContent("创建新端口"));
            final.Add(title);
            if (TargetPorts == null)
            {
                return final;
            }

            foreach (var port in TargetPorts)
            {
                final.Add(new SearchTreeEntry(new GUIContent(port.Name))
                {
                    level = 1,
                    userData = port
                });
            }

            return final;
        }

        public delegate bool SerchMenuWindowOnSelectEntryDelegate(SearchTreeEntry searchTreeEntry, SearchWindowContext context);            //声明一个delegate类
 
        public SerchMenuWindowOnSelectEntryDelegate OnSelectEntryHandler;                              //delegate回调方法
        

        public List<Type> TargetPorts;
        
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(searchTreeEntry, context);
        }
    }
}