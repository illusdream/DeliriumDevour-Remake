using System;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using StatModel;
using UnityEngine;

public class ActorMovement : MonoBehaviour,IBindingTarget
{
        public Actor actor;
        
        //应该朝向的方向
        Quaternion rotation = Quaternion.identity;

        public float TrunSpeed = 360;
        
        [ShowInInspector]
        public bool ApplyRootMotion { get;private set; }
        
        public bool ApplyRootMotionYSpeed { get; set; }
        
        public AnimationHandler AnimationHandler;
        
        /// <summary>
        /// 移动属性相关
        /// </summary>
        public BaseActorMoveStatModel MoveStatModel;
        
        public CharacterController characterController;

        public void FixedUpdate()
        {
                characterController?.Move(MoveStatModel.Velocity * Time.fixedDeltaTime);
        }

        public void UpdateTurn(Vector3 direction,bool immediate = false)
        {
                if (direction.sqrMagnitude > 0.01f)
                {
                        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                        if (immediate)
                        {
                                rotation = targetRotation;
                                return;
                        }
                        
                        rotation = Quaternion.RotateTowards(
                                rotation,
                                targetRotation,
                                GetTurnSpeed() * Time.deltaTime);
                }
        }

        public float GetTurnSpeed()
        {
                //每秒旋转两圈
                return TrunSpeed;
        }

        public void OnEnable()
        {
                AnimationHandler.RootMotionApply += OnRootMotionApply;
        }

        public void Update()
        {
                actor.transform.rotation = rotation;
        }

        public void OnDisable()
        {
                AnimationHandler.RootMotionApply -= OnRootMotionApply;
        }


        public void SetApplyRootMotion(bool value)
        {
                ApplyRootMotion = value;
                AnimationHandler.ApplyRootMotion = value;
        }
        
        private void OnRootMotionApply(Animator animator,Vector3 arg1, Quaternion arg2)
        {
               MoveStatModel.SetBaseVelocity(animator.velocity * MoveStatModel.RootMotionDisplacementMultor,ApplyRootMotionYSpeed);
               rotation *= arg2;
        }
        //用来处理RootMotion的
        [ShowInInspector]
        public string BindingName { get; }
        public Type BindingType { get; }
}