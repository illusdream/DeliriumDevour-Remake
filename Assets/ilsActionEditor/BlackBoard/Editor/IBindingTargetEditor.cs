using System;
using ilsFramework.Core;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

//TODO
//仍待完善
namespace ilsActionEditor.BlackBoard.Editor
{
    [InitializeOnLoad]
    public static class IBindingTargetContextAddition
    {
        static IBindingTargetContextAddition()
        {
        }

        [MenuItem("CONTEXT/Component/ActionEditor/AddToBlackBoard")]
        private static void AddToBlackBoard(MenuCommand command)
        {
            if (command.context is MonoBehaviour monoBehaviour and IBindingTarget iBindingTarget)
            {
                var blackboard = GetBlackBoardInParent(monoBehaviour);

                var BindingInstance = Activator.CreateInstance(iBindingTarget.BindingType) as IBlackBoardBinding;
                
            }
        }
        [MenuItem("CONTEXT/Component/ActionEditor/AddToBlackBoard",true)]
        public static bool ValidateAddToBlackBoard(MenuCommand command)
        {
            return command.context is  IBindingTarget;
        }

        private static MonoBlackBoard GetBlackBoardInParent(MonoBehaviour monoBehaviour)
        {
            MonoBlackBoard result = null;
            var currentTransform = monoBehaviour.transform;
            while (currentTransform)
            {
                if (currentTransform.TryGetComponent<MonoBlackBoard>(out var monoBlackBoard))
                {
                    result = monoBlackBoard;
                }
                currentTransform = currentTransform.parent;
            }

            return result;
        }
    }
}