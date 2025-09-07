using System;
using UnityEngine;

namespace ilsActionEditor
{
    public abstract class ABBValue
    {
        public abstract Type valueType { get; }
        
        public abstract object GetObjectValue();
    }

    public interface IBBValue<T>
    {
        T GetValue();
        
        void SetValue(IBBValue<T> value);
    }

    public  class BaseBBValue<T> : ABBValue, IBBValue<T>
    {
        public BaseBBValue(T value)
        {
            Value = value;
        }
        
        public T Value;
        public T GetValue()
        {
            return Value;
        }

        public override Type valueType =>typeof(T);
        public override object GetObjectValue()
        {
            return Value;
        }

        public void SetValue(IBBValue<T> value)
        {
            Value = value.GetValue();
        }

        public void SetValue(T value)
        {
            Value = value;
        }
    }
}