using System;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : System.Attribute
    {
        public Type NodeType;

        public CustomNodeEditorAttribute(Type nodeType)
        {
            this.NodeType = nodeType;
        }
    }
}