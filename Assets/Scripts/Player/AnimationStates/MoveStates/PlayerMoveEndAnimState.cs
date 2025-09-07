using Animancer;
using ilsActionEditor;
using UnityEngine;

[NodeMenuItem("Player/Animation/Move/Stop")]
public class PlayerMoveEndAnimState : BasePlayerAnimState
{
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation OnAnimEnd;

        [ExitTranslation] [SerializeReference] public BaseFuncTranslation ReMove;

        private bool AnimEnd;
        public override void OnInit()
        {
                base.OnInit();
                OnAnimEnd.condition += OnAnimEndCheck;
                ReMove.condition += OnRemoveCheck;
        }

        private bool OnRemoveCheck()
        {
                return !Mathf.Approximately(MoveStatModel.Speed.GetValue(), 0);
        }

        private bool OnAnimEndCheck()
        {
                return AnimEnd;
        }

        public override void OnEnter()
        {
                AnimEnd = false;
                ITransition TargetAnim;
                if (MoveStatModel.Speed.Value > playerController.playerBaseStat.MoveBufferStopSpeed)
                {
                        TargetAnim = GetCurrectLeftOrRightAnim(animationHandler.actorLocomotionSets.Move_End_Left_Buffer_Anim, animationHandler.actorLocomotionSets.Move_End_Right_Buffer_Anim);
                }
                else
                {
                        TargetAnim = GetCurrectLeftOrRightAnim(animationHandler.actorLocomotionSets.Move_End_Left_Anim, animationHandler.actorLocomotionSets.Move_End_Right_Anim);
                }
                CurrentAnimState = animationHandler.Play(TargetAnim, TargetLayer);
                CurrentAnimState.Events(this).OnEnd += OnEnd;
                base.OnEnter();
        }

        private void OnEnd()
        {
                AnimEnd = true;
                CurrentAnimState.Events(this).OnEnd -= OnEnd;
        }
}