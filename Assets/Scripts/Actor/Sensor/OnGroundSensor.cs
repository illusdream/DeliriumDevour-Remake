using System;
using System.Collections.Generic;
using Animancer.Units;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class OnGroundSensor : MonoBehaviour,IBindingTarget
{
        public static string BlackBoardKey = "OnGroundSensor";
        
        public string BindingName => "OnGroundSensor";
        public Type BindingType { get; }
        
        public Vector3 SenserOffset;
        
        public float Radius;

        private Collider[] SensorBuffer;
        
        public LayerMask GroundLayer;
        
        private bool _onGround;
        
        private TimerCollection timerCollection;
        
        [Seconds]
        public float coyoteTime;
        
        [ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
        public bool OnGround
        {
                get => _onGround;
                private set
                {
                        if (_onGround == true && value ==false)
                        {
                                OnLeaveGround?.Invoke();
                                timerCollection.CreateTimer(coyoteTime,1,"Coyote").SetOnCompleted((t) =>
                                {
                                        coyoteInGround = false;
                                }).Register();
                        }

                        if (_onGround == false && value == true)
                        {
                                OnTouchGround?.Invoke();
                                coyoteInGround = true;
                                timerCollection.ClearAllTimers();
                        }
                        _onGround = value;
                }
        }

        private bool coyoteInGround;
        
        [ShowInInspector]
        public bool CoyoteBuffer=>coyoteInGround;
        
        public event Action OnTouchGround;
        
        public event Action OnLeaveGround;

        public void Awake()
        {
                timerCollection = new TimerCollection();
                SensorBuffer = new Collider[5];
        }

        public void FixedUpdate()
        {
                var result =  Physics.OverlapSphereNonAlloc(transform.position + SenserOffset, Radius,SensorBuffer, GroundLayer);
                OnGround = result > 0;
        }

        public void OnDrawGizmos()
        {
                Gizmos.DrawWireSphere(transform.position + SenserOffset, Radius);
        }

}