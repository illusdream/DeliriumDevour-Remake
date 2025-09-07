using System;
using UnityEngine;

namespace ilsActionEditor
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NodeColorAttribute : Attribute
    {
        public NodeColorAttribute(float r, float g, float b,float a)
        {
            NodeColor = new Color(r, g, b,a);
        }

        public Color NodeColor { get; private set; }
        
    }
}