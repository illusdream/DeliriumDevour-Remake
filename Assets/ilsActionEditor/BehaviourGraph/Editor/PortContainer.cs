using System;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor.Editor
{
    public class PortContainer : ScriptableObject
    {
        [SerializeReference]
        [ShowInInspector]
        [HideLabel] 
        [HideReferenceObjectPicker]
        [OnValueChanged("valueChange", true)]
        public AFSMStateTranslation transition;

        public void valueChange()
        {
            PortInspectorValueChange?.Invoke();
        }
        
        public event Action PortInspectorValueChange;
    }
}