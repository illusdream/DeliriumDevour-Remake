using System;
using System.Collections.Generic;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphCategoryAttribute : Attribute
    {
        public HashSet<string> Categories;

        public GraphCategoryAttribute(params string[] categories)
        {
            Categories = new HashSet<string>(categories);
        }
    }
}