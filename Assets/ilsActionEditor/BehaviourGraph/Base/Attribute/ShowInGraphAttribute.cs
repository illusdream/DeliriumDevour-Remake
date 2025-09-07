using System;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = true, Inherited = true)]
    public class ShowInGraphAttribute : Attribute
    {
        
    }
}