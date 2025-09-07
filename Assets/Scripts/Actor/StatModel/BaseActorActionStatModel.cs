using System;
using System.Collections.Generic;
using ilsActionEditor;
using Sirenix.OdinInspector;

namespace StatModel
{
    public class BaseActorActionStatModel : BaseActorStatModel,IBindingTarget
    {
        public static string BlackBoardKey = "ActionStatModel";
        
        public string BindingName =>BlackBoardKey;
        public Type BindingType { get; }

        //
        public HashSet<string> StartUpCancels;
        
        public HashSet<string> ExecutionCancels;
        
        [ShowInInspector]
        public HashSet<string> RecoveryCancels;
        [ShowInInspector]
        public HashSet<string> BlockInput;

        public bool OnAction;
        
        public void Awake()
        {
            StartUpCancels = new HashSet<string>();
            ExecutionCancels = new HashSet<string>();
            RecoveryCancels = new HashSet<string>();
            BlockInput = new HashSet<string>();
        }
    }
}