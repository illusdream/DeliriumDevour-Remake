using DefaultNamespace;
using ilsActionEditor;
using StatModel;
using UnityEngine;
using UnityEngine.Serialization;

[NodeMenuItem("Player/GroundState")]
public class PlayerGroundState : SubAFSMNode
{
        [FormerlySerializedAs("playerCheckJumpTranslation")]
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation Jump;
        
        
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation InAir;

        public OnGroundSensor groundSensor;
        
        public PlayerInputHandler playerInputHandler;
        
        public BaseActorMoveStatModel actorMoveStatModel;
        
        public override void OnInit()
        {
                Jump.condition += CheckJump;
                InAir.condition += CheckInAir;
                groundSensor = BlackBoard.GetValue<OnGroundSensor>(OnGroundSensor.BlackBoardKey);
                playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
                actorMoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
                base.OnInit();
        }

        private bool CheckInAir()
        {
                return !groundSensor.CoyoteBuffer && actorMoveStatModel.YSpeed.Value < -9.8f * 2*Time.fixedDeltaTime;
        }

        private bool CheckJump()
        {
                return groundSensor.OnGround && playerInputHandler.HasTriggered(EPlayerInput.Jump, EInputInteraction.Tap, 0.2f);
        }
}