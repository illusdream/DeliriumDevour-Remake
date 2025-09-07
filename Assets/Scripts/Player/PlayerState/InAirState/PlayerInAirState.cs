using Animancer;
using ilsActionEditor;
using StatModel;
using UnityEngine;

[NodeMenuItem("Player/InAir/InAirState")]
public class PlayerInAirState : BasePlayerState
{
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation OnGround;
        
        
        public AnimancerState Jump;
        public override void OnInit()
        {
                base.OnInit();
                OnGround.condition += CheckOnGround;
                animationHandler.ApplyRootMotion = false;
                playerController.actorMovement.SetApplyRootMotion(false);
        }

        public override void OnEnter()
        {
                //判断是左脚还是右脚
                Transform leftFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rightFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.RightFoot);

                Vector3 leftFootLocalPos = playerTransform.InverseTransformPoint(leftFoot.position);
                Vector3 rightFootLocalPos = playerTransform.InverseTransformPoint(rightFoot.position);
                
                var targetV2 = playerController.ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue);
                float inputAngle = Mathf.Atan2(targetV2.x, targetV2.z) * Mathf.Rad2Deg;
                float playerAngle = Mathf.Atan2(MoveStatModel.FaceDirection.x, MoveStatModel.FaceDirection.z) * Mathf.Rad2Deg;
                float angleDelta = Mathf.DeltaAngle(playerAngle, inputAngle);
                //正负代表右左
                
                if (leftFootLocalPos.z > rightFootLocalPos.z)
                {
                      //  animationHandler.Play(playerController.playerBaseStat.InAir_L);
                }
                else
                {
                      //  animationHandler.Play(playerController.playerBaseStat.InAir_R);
                }
                
                
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
                var dir =  playerController.ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue.normalized);
                velocity += new Vector2(dir.x,dir.z) * (playerController.playerBaseStat.Acc * Time.fixedDeltaTime);
                
                velocity =Vector2.ClampMagnitude(velocity, playerController.playerBaseStat.BaseMoveSpeed);
                
                MoveStatModel.SetBaseVelocity(new Vector3(velocity.x,0,velocity.y),false);
                base.OnLogicUpdate();
        }

        public override void OnFixedUpdate()
        {
               //  var dir = playerController.ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue.normalized);
               //  MoveStatModel.HorizontalNormalizedVelocity =new Vector2(dir.x,dir.z);
               // MoveStatModel.Speed.BaseValue =  playerController.playerBaseStat.BaseMoveSpeed * Mathf.Clamp01(playerInputHandler.Move.ActionValue.magnitude);
                base.OnFixedUpdate();
        }

        private bool CheckOnGround()
        {
                return onGroundSensor.OnGround;
        }
}