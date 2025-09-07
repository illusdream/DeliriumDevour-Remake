using System;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomPortEditorAttribute : Attribute
    {
        public Type NodeType;

        public CustomPortEditorAttribute(Type nodeType)
        {
            this.NodeType = nodeType;
        }
    }
}