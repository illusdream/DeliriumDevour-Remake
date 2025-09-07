using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using ilsFramework;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

namespace ilsActionEditor
{
    public class MonoBlackBoard : MonoBehaviour
    {
        [ShowInInspector]
        public BlackBoard blackBoard;

        [SerializeReference]
        [HideLabel]
        [HideReferenceObjectPicker]
        public List<IBlackBoardBinding> blackBoardBindings = new List<IBlackBoardBinding>();
        
        
        public void Awake()
        {
            blackBoard = new BlackBoard();

            foreach (var blackBoardBinding in blackBoardBindings)
            {

                blackBoardBinding.GetBinding(out string key, out object value );
                blackBoard.InitValue(key, value);
            }
        }

        public T GetValue<T>(string key)
        {
            return blackBoard.GetValue<T>(key);
        }

        public void SetValue<T>(string key, T value)
        {
            blackBoard.SetValue(key, value);
        }
        
        

        public void SetBlackBoardBinding(IBlackBoardBinding binding)
        {
            if (!blackBoardBindings.Any(t =>
                {
                    t.GetBinding(out string key, out object _);
                    binding.GetBinding(out string key2, out object _);
                    return key == key2;
                }))
            {
                blackBoardBindings.Add(binding);
            }
        }
    }
    
    public interface IBlackBoardBinding
    {
        void GetBinding(out string key, out object value);
    }
    


    
}