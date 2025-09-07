using System;

namespace ilsActionEditor
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true,Inherited = true)]
    public class NodeWidthAttribute : Attribute
    {
        public float NodeWidth { get; set; }

        public NodeWidthAttribute(float nodeWidth)
        {
            this.NodeWidth = nodeWidth;
        }
    }
}