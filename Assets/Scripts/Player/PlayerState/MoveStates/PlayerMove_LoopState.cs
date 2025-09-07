using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using StatModel;
using UnityEngine;

[NodeMenuItem("Player/Move/LoopState")]
public class PlayerMove_LoopState : BasePlayerState
{
        [ExitTranslation]
        [SerializeReference]
        public PlayerMoveTranslation OnNoInputMove;
        

        public override void OnInit()
        {
                base.OnInit();
        }

        public override void OnEnter()
        {
                base.OnEnter();
        }


        public override void OnUpdate(float deltaTime)
        {
                playerController.UpdateMovement(playerInputHandler.Move.ActionValue);
                base.OnUpdate(deltaTime);
        }

        public override void OnLogicUpdate()
        {
                MoveStatModel.HorizontalNormalizedVelocity = new Vector2(MoveStatModel.FaceDirection.x,MoveStatModel.FaceDirection.z).normalized;
                
                var velocity = MoveStatModel.HorizontalNormalizedVelocity * MoveStatModel.Speed.BaseValue;
                var dir =  playerController.ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue);
                velocity += new Vector2(dir.x,dir.z) * (playerController.playerBaseStat.Acc * Time.fixedDeltaTime);
                
                velocity =Vector2.ClampMagnitude(velocity, playerController.playerBaseStat.BaseMoveSpeed);
                
                MoveStatModel.SetBaseVelocity(new Vector3(velocity.x,0,velocity.y),false);
                
                base.OnLogicUpdate();
        }
        
        public override void OnExit()
        {
                base.OnExit();
        }
}