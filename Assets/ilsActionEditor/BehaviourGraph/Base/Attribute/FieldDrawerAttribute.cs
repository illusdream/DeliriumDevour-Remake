using System;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FieldDrawerAttribute : Attribute
    {
        public Type		fieldType;

        /// <summary>
        /// Register a custom view for a type in the FieldFactory class
        /// </summary>
        /// <param name="fieldType"></param>
        public FieldDrawerAttribute(Type fieldType)
        {
            this.fieldType = fieldType;
        }
    }
}