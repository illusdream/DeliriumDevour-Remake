using System;

namespace ilsActionEditor
{
    public interface IBindingTarget
    {
        public string BindingName { get; }
        public Type BindingType { get; }
    }
}