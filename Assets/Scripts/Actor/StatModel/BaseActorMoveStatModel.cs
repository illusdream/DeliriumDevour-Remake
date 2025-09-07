using System;
using ilsActionEditor;
using ProcessorPipelines;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatModel
{
    //存储数据
    public class BaseActorMoveStatModel : BaseActorStatModel,IBindingTarget
    {
        public static string BlockBoardKey = "ActionStatModel";

        public string BindingName => BlockBoardKey;
        public Type BindingType { get; }
        
        public OnGroundSensor OnGroundSensor;
        
        public static string BlackBoardKey = "ActorMoveStatModel";
        //是否可以移动
        public bool CanMove { get; set; }
        
        //移动方向
        public Vector3 MoveDirection=>Velocity.normalized;
        
        public Vector3 FaceDirection=> transform.forward;
        
        public Vector3 Position { get; set; }
        
        //速度
        [ShowInInspector]
        public OldStat<Common1Pipeline,float> Speed { get; set; }
        
        public Vector2 HorizontalNormalizedVelocity { get; set; }

        [ShowInInspector]
        public OldStat<Common1Pipeline, float> YSpeed;

        public Vector3 Velocity
        {
            get
            {
                var hSpeed = Speed.Value;
                return new(HorizontalNormalizedVelocity.x *hSpeed, YSpeed.Value, HorizontalNormalizedVelocity.y * hSpeed);
            }
        }

        public float RootMotionDisplacementMultor = 1;
        
        public bool OnGround => OnGroundSensor.OnGround;
        
        public float GravityMultor = 1;

        public void Awake()
        {
            Speed = new OldStat<Common1Pipeline, float>(0);
            YSpeed = new OldStat<Common1Pipeline, float>(0);
        }

        public void FixedUpdate()
        {
            UpdateGravity();
        }

        public void SetBaseVelocity(Vector3 velocity,bool ApplyYSpeed = true)
        {
            var hVelocity = new Vector2(velocity.x, velocity.z);
            HorizontalNormalizedVelocity = hVelocity.normalized;
            if (ApplyYSpeed)
            {
                YSpeed.BaseValue = velocity.y;
            }
            Speed.BaseValue = hVelocity.magnitude;
        }

        public void OnDrawGizmos()
        {
            //Gizmos.DrawLine(transform.position, transform.position + FaceDirection);
        }

        private void UpdateGravity()
        {
            var GravityDelta = -9.8f * GravityMultor * Time.fixedDeltaTime;
            if (OnGround)
            {
                if (YSpeed.BaseValue < GravityDelta)
                {
                    YSpeed.BaseValue = GravityDelta;
                }
            }
            else
            {
                YSpeed.BaseValue += GravityDelta;
            }
        }


    }
}