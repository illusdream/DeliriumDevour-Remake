using System;
using System.Collections.Generic;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    public class BlackBoard
    {
        [ShowInInspector]
        private Dictionary<string,ABBValue> keyValues = new Dictionary<string, ABBValue>();

        public T GetValue<T>(string key)
        {
            if (keyValues.TryGetValue(key, out ABBValue value))
            {
                if (value is BaseBBValue<T> result)
                {
                    return result.Value;
                }
                if (value.GetObjectValue() is T final)
                {
                    return final;
                }
            }
            $"{key} is not a valid value".LogSelf();
            return default(T);
        }

        public void SetValue<T>(string key, T value)
        {
            if (keyValues.TryGetValue(key,out var result))
            {
                (result as BaseBBValue<T>)?.SetValue(value);
            }
            else
            {
                keyValues[key] = GetCurrectBBValueType(value);
            }
        }

        public void RemoveValue(string key)
        {
            keyValues.Remove(key);
        }

        public void InitValue(string key, object value)
        {
            keyValues[key] = InitGetCurrectBBValueType(value);
        }


        private ABBValue GetCurrectBBValueType<T>(T value)
        {
            
            switch (value)
            {
                case int i:
                    return new BBValue_Int(i);
                    break;
                case float f:
                    return new BBValue_Float(f);
                    break;
                case bool _bool:
                    return new BBValue_Bool(_bool);
                    break;
                case string _string:
                    return new BBValue_String(_string);
                    break;
                case Vector3 _vector:
                    return new BBValue_Vector3(_vector);
                    break;
                default:
                    return new BaseBBValue<T>(value);
                case null:
                    throw new ArgumentException("黑板设置值为null");
            }
        }
        
        private ABBValue InitGetCurrectBBValueType<T>(T value)
        {
            
            switch (value)
            {
                case int i:
                    return new BBValue_Int(i);
                    break;
                case float f:
                    return new BBValue_Float(f);
                    break;
                case bool _bool:
                    return new BBValue_Bool(_bool);
                    break;
                case string _string:
                    return new BBValue_String(_string);
                    break;
                case Vector3 _vector:
                    return new BBValue_Vector3(_vector);
                    break;
                default:
                    //这里修改一下，因为最终这里的T是object，所以要强制转换成对应的类型
                    var targetBBValueType = typeof(BaseBBValue<>).MakeGenericType(value.GetType());
                    var instance = (ABBValue)Activator.CreateInstance(targetBBValueType,value);
                    return instance;
                case null:
                    throw new ArgumentException("黑板设置值为null");
            }
        }
    }
}