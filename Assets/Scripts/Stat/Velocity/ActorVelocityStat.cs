using UnityEngine;

//大致流程 根据水平初速度 设置影响水平速度大小 设置水平速度范围 设置Y轴速度 设置影响Y轴大小的范围，设置最大下落速度相关的
public class ActorVelocityStat
{
        public Vector3 CurrentVelocity;
        
        public Vector2 HorizontalVelocity;

        public float YSpeed;
}