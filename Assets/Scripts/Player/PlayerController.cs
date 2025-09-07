using System;
using Animancer;
using Cinemachine;
using ilsActionEditor;
using ilsFramework.Core;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
        public PlayerInputHandler playerInputHandler;

        public ActorMovement actorMovement;

        public CinemachineFreeLook playerCamera;
        
        public AnimancerComponent animancerComponent;

        public MonoBlackBoard blackBoard;
        
        public PlayerBaseStat playerBaseStat;
        public void Awake()
        {

        }
        public void Start()
        {
        }
        public void Update()
        {
                // animancerComponent.Evaluate();

                //animancerComponent.Animator.deltaPosition.LogSelf();
                // UpdateMovement();
        }

        public void UpdateMovement(Vector3? targetMoveDir = null,float speed = 0)
        {
                if (targetMoveDir is null)
                {
                        actorMovement.UpdateTurn(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z));
                }
                else
                {
                        var move = ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue);
                        actorMovement.UpdateTurn(move);
                }
                //actorMovement.MoveStatModel.Velocity.BaseValue = speed;
                //transform.position += move*Time.deltaTime;

        }

        public void UpdateTurnImmediate(Vector3 targetMoveDir)
        {
                actorMovement.UpdateTurn(targetMoveDir,true);
        }

        public Vector3 ConvertFromCameraLocal2World(Vector2 move)
        {
                var movement = move.magnitude;
                if (movement > 0.01f)
                {
                        var direction = Camera.main.transform.TransformDirection(new Vector3(move.x, 0, move.y));
                        direction.y = 0;
                        direction.Normalize();
                        return direction;
                }
                return Vector3.zero;
        }

        public void OnAnimatorMove()
        {
                
        }

        public void OnDrawGizmos()
        {
                var left = animancerComponent.Animator.GetBoneTransform(HumanBodyBones.LeftToes);
                Gizmos.DrawRay(left.position, - Vector3.up);
                
                var right = animancerComponent.Animator.GetBoneTransform(HumanBodyBones.RightToes);
                Gizmos.DrawRay(right.position, - Vector3.up);
        }
}