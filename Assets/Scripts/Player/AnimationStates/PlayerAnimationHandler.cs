using System;
using Animancer;
using StatModel;
using UnityEditor;
using UnityEngine;

public class PlayerAnimationHandler : AnimationHandler
{
    private SmoothedFloatParameter SmoothedDeltaArcParam;
    private Vector2 OldFaceDir;

    public BaseActorMoveStatModel MoveStatModel;

    public float DeltaAngleMultilper;

    public PlayerController PlayerController;
    public override void Start()
    {
        base.Start();
        SmoothedDeltaArcParam = new SmoothedFloatParameter(animancerComponent, actorLocomotionSets.Move_DeltaArc_Parameter, 0.2f);

    }

    public override void Update()
    {
        UpdateParameter();
        base.Update();
    }

    private void UpdateParameter()
    {
        animancerComponent.Parameters.SetValue(actorLocomotionSets.Move_Speed_Parameter,MoveStatModel.Speed.Value);
        float inputAngle = Mathf.Atan2(OldFaceDir.x, OldFaceDir.y) * Mathf.Rad2Deg;
        float playerAngle = Mathf.Atan2(MoveStatModel.FaceDirection.x, MoveStatModel.FaceDirection.z) * Mathf.Rad2Deg;
        float angleDelta = -Mathf.DeltaAngle(playerAngle, inputAngle) * DeltaAngleMultilper;

        SmoothedDeltaArcParam.TargetValue = Mathf.Clamp(angleDelta, -1, 1);
        //方向改成追向速度方向
        OldFaceDir =new Vector2(MoveStatModel.FaceDirection.x,MoveStatModel.FaceDirection.z).normalized;
    }

    public void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            var distance = Mathf.Pow(MoveStatModel.Speed.Value, 2) / PlayerController.playerBaseStat.Acc/2;
        
            Gizmos.DrawRay(PlayerController.transform.position, PlayerController.transform.forward * distance);
        }

    }
}