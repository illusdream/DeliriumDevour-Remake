using ilsActionEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerInAirAnimState : BasePlayerAnimState
    {
        [SerializeReference]
        [ExitTranslation]
        public BaseFuncTranslation ToGround;

        public override void OnInit()
        {
            base.OnInit();
            ToGround.condition += Condition;
        }

        private bool Condition()
        {
            return MoveStatModel.OnGround;
        }
        public override void OnEnter()
        {
            var targetAnim = GetCurrectLeftOrRightAnim(animationHandler.actorLocomotionSets.InAir_L, animationHandler.actorLocomotionSets.InAir_R);
            CurrentAnimState = animationHandler.Play(targetAnim, TargetLayer);
            
            base.OnEnter();
        }
    }
}