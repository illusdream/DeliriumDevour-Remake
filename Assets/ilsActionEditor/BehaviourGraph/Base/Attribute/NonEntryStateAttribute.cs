using System;

namespace ilsActionEditor
{
    /// <summary>
    /// 被这个标记的类无法作为状态机的Entry
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NonEntryStateAttribute : Attribute
    {
        
    }
}