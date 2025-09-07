using System;
using System.Collections.Generic;
using System.Linq;
using ilsActionEditor.Node;
using ilsFramework.Core;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace ilsActionEditor.Editor
{
    /// <summary>
    /// 缓存器，用来把Node和对应的View类关联起来
    /// </summary>
    public class BehaviourGraphCache
    {
        private static BehaviourGraphCache _instance;

        public static BehaviourGraphCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BehaviourGraphCache();
                    _instance.InitAllCacheData();
                    _instance.InitNodeGraphMenuItemAttributeCache();
                }
                return _instance;
            }
        }

        //应该不会有通过View查找Node的需求吧
        public Dictionary<Type,Type> Node_NodeViewCache = new Dictionary<Type, Type>();
        
        public Dictionary<Type,Type> Translation_PortViewCache = new Dictionary<Type, Type>();
        
        //用来查询的前缀树
        public SortedSet<(string,Type)> NodeMenuItemSortedSet = new SortedSet<(string,Type)>(new NodeMenuItemStringComparer());
        
        
        
        public BehaviourGraphCache()
        {
            
        }

        ~BehaviourGraphCache()
        {
            
        }

        private void OnSelectionChange()
        {
            
        }

        private void InitAllCacheData()
        {
            InitNodeViewCache();
            InitPortViewCache();
        }

        private void InitNodeViewCache()
        {
            //优先查找所有继承了View的类，找不到的话再用基础的Node
            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
            {
                if (nodeViewType.IsDefined(typeof(CustomNodeEditorAttribute),false))
                {
                    var attrs = nodeViewType.GetCustomAttributes(typeof(CustomNodeEditorAttribute), false);
                    var cnAttrs = attrs.FirstOrDefault(attr => attr is CustomNodeEditorAttribute) as CustomNodeEditorAttribute;
                    if (cnAttrs != null)
                    {
                        Node_NodeViewCache.Add(cnAttrs.NodeType, nodeViewType);
                    }
                }
            }
            
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseBehaviourNode>())
            {
                if (!Node_NodeViewCache.ContainsKey(nodeType))
                {
                    bool find = false;
                    var currentType = nodeType;
                    //遍历查找
                    while (currentType != typeof(BaseBehaviourNode))
                    {
                        if (Node_NodeViewCache.TryGetValue(currentType,out var editorType))
                        {
                            Node_NodeViewCache.Add(nodeType, editorType);
                            find = true;
                            break;
                        }
                        currentType = currentType.BaseType;
                    }

                    if (find)
                    {
                        continue;
                    }
                    else
                    {
                        Node_NodeViewCache.Add(nodeType,typeof(BaseNodeView));
                    }
                }
              
            }
        }

        private void InitPortViewCache()
        {
            //优先查找所有继承了View的类，找不到的话再用基础的Port
            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BasePortView>())
            {
                if (nodeViewType.IsDefined(typeof(CustomPortEditorAttribute),false))
                {
                    var attrs = nodeViewType.GetCustomAttributes(typeof(CustomPortEditorAttribute), false);
                    if (attrs.FirstOrDefault(attr => attr is CustomPortEditorAttribute) is CustomPortEditorAttribute cnAttrs)
                    {
                        Translation_PortViewCache.Add(cnAttrs.NodeType, nodeViewType);
                    }
                }
            }
            
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<AFSMStateTranslation>())
            {
                if (!Translation_PortViewCache.ContainsKey(nodeType))
                {
                    bool find = false;
                    var currentType = nodeType;
                    //遍历查找
                    while (currentType != typeof(AFSMStateTranslation) && currentType != null)
                    {
                        if (Translation_PortViewCache.TryGetValue(currentType,out var editorType))
                        {
                            Translation_PortViewCache.Add(nodeType, editorType);
                            find = true;
                            break;
                        }
                        currentType = currentType.BaseType;
                    }

                    if (find)
                    {
                        continue;
                    }
                    else
                    {
                        Translation_PortViewCache.Add(nodeType,typeof(BasePortView));
                    }
                }
              
            }
        }

        private void InitNodeGraphMenuItemAttributeCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseBehaviourNode>())
            {
                if (nodeType.IsAbstract ||nodeType.IsGenericType)
                {
                 continue;   
                }
                if (nodeType.IsDefined(typeof(NodeMenuItemAttribute),false))
                {
                    var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false);
                    var cnAttrs = attrs.FirstOrDefault(attr => attr is NodeMenuItemAttribute) as NodeMenuItemAttribute;
                    if (cnAttrs != null)
                    {
                        NodeMenuItemSortedSet.Add((cnAttrs.menuItemName,nodeType));
                    }
                }
                else
                {
                    //放到Other中
                    NodeMenuItemSortedSet.Add(($"Other/{nodeType.Name}",nodeType));
                }
            }
        }



        public Type GetNodeViewType(Type nodeType)
        {
            return Node_NodeViewCache[nodeType];
        }

        public Type GetPortViewType(Type translationType)
        {
            return Translation_PortViewCache[translationType];
        }

        public List<(string,Type)> GetGraphContainsNodeType<T>() where T : BaseBehaviourGraph
        {
            var result = new List<(string,Type)>();
            if (typeof(T).IsDefined(typeof(GraphCategoryAttribute),false))
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(GraphCategoryAttribute), false);
                var cnAttrs = attrs.FirstOrDefault(attr => attr is GraphCategoryAttribute) as GraphCategoryAttribute;
                if (cnAttrs != null)
                {
                    foreach (var category in cnAttrs.Categories)
                    {
                        // 构造范围边界
                        var start = (category, null as Type);
                        var end = (category + char.MaxValue, null as Type);

                        result.AddRange(NodeMenuItemSortedSet.GetViewBetween(start, end));
                    }
                }
            }
            else
            {
                result.AddRange(NodeMenuItemSortedSet);
            }
            return result;
        }
        public List<(string,Type)> GetGraphContainsNodeType(string category)
        {
            var result = new List<(string,Type)>();
            // 构造范围边界
            var start = (category, null as Type);
            var end = (category + char.MaxValue, null as Type);

            result.AddRange(NodeMenuItemSortedSet.GetViewBetween(start, end));
            return result;
        }

        public List<(string,Type)> GetAllNodeTypes()
        {
            return NodeMenuItemSortedSet.ToList();
        }
    }
    
    public class NodeMenuItemStringComparer : IComparer<(string, Type)>
    {
        public int Compare((string, Type) x, (string, Type) y)
        {
            // 仅比较字符串部分（忽略Type）
            return string.Compare(x.Item1, y.Item1, StringComparison.Ordinal);
        }
    }
}