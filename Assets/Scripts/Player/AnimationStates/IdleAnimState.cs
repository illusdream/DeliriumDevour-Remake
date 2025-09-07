using Animancer;
using ilsActionEditor;
using UnityEngine;

[NodeMenuItem("Player/Animation/Idle")]
public class IdleAnimState : BasePlayerAnimState
{
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation ToMove;
        
        
        public override void OnEnter()
        {
                var targetAnim = GetCurrectLeftOrRightAnim(animationHandler.actorLocomotionSets.Idle_Left_Anim, animationHandler.actorLocomotionSets.Idle_Right_Anim);
                CurrentAnimState = animationHandler.Play(targetAnim, TargetLayer);
                
                ToMove.condition += CheckToMove;
                base.OnEnter();
        }

        private bool CheckToMove()
        {
                return !Mathf.Approximately(MoveStatModel.Speed.GetValue(), 0);
        }

        public override void OnUpdate(float deltaTime)
        {
                base.OnUpdate(deltaTime);
        }

        public override void OnExit()
        {
                base.OnExit();
        }
}