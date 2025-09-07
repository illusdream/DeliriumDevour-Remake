using System;
using System.Collections.Generic;
using System.Linq;
using ilsActionEditor.Editor;
using ilsFramework.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ilsActionEditor
{
    public class BehaviourSearchMenuWindowProvider : ScriptableObject,ISearchWindowProvider
    {
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var final = new List<SearchTreeEntry>();
            Dictionary<string,SearchTreeEntry> searchTree = new Dictionary<string,SearchTreeEntry>();
            var title = new SearchTreeGroupEntry(new GUIContent("创建新节点"));
            final.Add(title);
            if (GraphType == null)
            {
                return final;
            }

            if (GraphType.IsDefined(typeof(GraphCategoryAttribute),false))
            {
                var attrs = GraphType.GetCustomAttributes(typeof(GraphCategoryAttribute), false);
                var cnAttrs = attrs.FirstOrDefault(attr => attr is GraphCategoryAttribute) as GraphCategoryAttribute;
                if (cnAttrs != null)
                {
                    switch (cnAttrs.Categories.Count)
                    {
                        case 0:
                            //直接把所有显示出来
                            foreach (var type in BehaviourGraphCache.Instance.GetAllNodeTypes())
                            {
                                CreateSingleEntry(type);
                            }
                            break;
                        case 1:
                        {
                            //把内容物都显示出来
                            var prefix = cnAttrs.Categories.First();
                            var result = BehaviourGraphCache.Instance.GetGraphContainsNodeType(prefix);
                            foreach (var data in result)
                            {
                                title.content.text ="创建新节点 in:" + prefix;
                                //相当于没有被分割
                                final.Add(new SearchTreeEntry(new GUIContent((data.Item1.Replace(prefix + "/",""))))
                                {
                                    level    = 1,
                                    userData = data.Item2
                                });
                            }
                            //Other的也要显示出来
                            var otherResult = BehaviourGraphCache.Instance.GetGraphContainsNodeType("Other");
                            foreach (var data in otherResult)
                            {
                                //相当于没有被分割
                                final.Add(new SearchTreeEntry(new GUIContent((data.Item1.Replace("Other" + "/",""))))
                                {
                                    level    = 1,
                                    userData = data.Item2
                                });
                            }
                        }
                            break;
                        case > 1:
                        {
                            foreach (var category in cnAttrs.Categories)
                            {
                                var result = BehaviourGraphCache.Instance.GetGraphContainsNodeType(category);
                                foreach (var data in result)
                                {
                                    CreateSingleEntry(data);
                                }
                            }
                            //Other的也要显示出来
                            var otherResult = BehaviourGraphCache.Instance.GetGraphContainsNodeType("Other");
                            foreach (var data in otherResult)
                            {
                                //相当于没有被分割
                                final.Add(new SearchTreeEntry(new GUIContent((data.Item1.Replace("Other" + "/",""))))
                                {
                                    level    = 1,
                                    userData = data.Item2
                                });
                            }
                        }
                            break;
                    }

                }
            }
            else
            {
                foreach (var type in BehaviourGraphCache.Instance.GetAllNodeTypes())
                {
                    CreateSingleEntry(type);
                }
            }
            //获取到当前graph的Type
            void CreateSingleEntry((string,Type) data)
            {
                //根据每一段创建然后循环处理
                //最后Right是Empty说明是结尾了
                var nodePath = data.Item1;
                var nodeName = nodePath;
                var level = 0;
                var parts = nodePath.Split('/');

                if (parts.Length > 0)
                {
                    level++;
                    nodeName = parts[parts.Length - 1];
                    var fullTitleAsPath = "";

                    for (int i = 0; i < parts.Length-1; i++)
                    {
                        var title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;
                        if (!searchTree.ContainsKey(fullTitleAsPath))
                        {
                            var result=  new SearchTreeGroupEntry(new GUIContent(title)){
                                level = level
                            };
                            final.Add(result);
                            searchTree.Add(fullTitleAsPath,result);
                        }
                    }
                }
                final.Add(new SearchTreeEntry(new GUIContent(parts.Last()))
                {
                    level    = level + 1,
                    userData = data.Item2
                });

            }
            
            return final;
        }

        
        public delegate bool SerchMenuWindowOnSelectEntryDelegate(SearchTreeEntry searchTreeEntry, SearchWindowContext context);            //声明一个delegate类
 
        public SerchMenuWindowOnSelectEntryDelegate OnSelectEntryHandler;                              //delegate回调方法

        public Type GraphType;

        public void SetGraphType(Type graphType)
        {
            GraphType = graphType;
        }
 
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(searchTreeEntry, context);
        }
        
        public  (string LeftPart, string RightPart) SplitAtLast(string input, char delimiter)
        {
            if (string.IsNullOrEmpty(input))
                return (input, string.Empty);

            int index = input.LastIndexOf(delimiter);
        
            if (index < 0) 
                return (input, string.Empty);
        
            return (
                LeftPart: input[..index],
                RightPart: input[(index + 1)..]
            );
        }
    }
}