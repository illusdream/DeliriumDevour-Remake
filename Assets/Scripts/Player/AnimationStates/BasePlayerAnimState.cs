using Animancer;
using ilsActionEditor;
using UnityEngine;

public class BasePlayerAnimState : BasePlayerState
{
        public AnimancerState CurrentAnimState;
        [ShowInGraph]
        public StringAsset TargetLayer;

        public ITransition GetCurrectLeftOrRightAnim(ITransition left, ITransition right)
        {
                //判断是左脚还是右脚
                Transform leftFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rightFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.RightFoot);

                Vector3 leftFootLocalPos = playerTransform.InverseTransformPoint(leftFoot.position);
                Vector3 rightFootLocalPos = playerTransform.InverseTransformPoint(rightFoot.position);
        
                return leftFootLocalPos.z > rightFootLocalPos.z ? left : right;
        }
}