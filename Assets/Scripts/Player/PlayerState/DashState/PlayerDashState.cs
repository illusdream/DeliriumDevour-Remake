using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using StatModel;
using UnityEngine;

[NodeMenuItem("Player/DashState")]
public class PlayerDashState : BaseActionPlayerState
{
    [ExitTranslation]
    [SerializeReference]
    public TestDashMoveCancelTranslation OtherOnInputMove;
    
    
    [ExitTranslation]
    [SerializeReference]
    public BaseConditionTranslation OnAnimationEnd;
    
    private Vector3 targetDirection;
    
    TimerCollection timer;

    public float lerpValue;
    public override void OnInit()
    {
        timer = new TimerCollection();
        base.OnInit();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        playerController.actorMovement.ApplyRootMotionYSpeed = true;
        OnAnimationEnd.Condition = false;
        animationHandler.ApplyRootMotion = true;
        targetDirection = playerController.ConvertFromCameraLocal2World(playerInputHandler.Move.ActionValue);
        timer.CreateTimer(0.2f, 1, "TurnAngle").SetOnCycling((t) =>
        {
            playerController.UpdateTurnImmediate(Vector3.Slerp(MoveStatModel.FaceDirection,targetDirection,lerpValue));
        }).Register();
        MoveStatModel.RootMotionDisplacementMultor =Mathf.Clamp( MoveStatModel.Speed.Value / playerController.playerBaseStat.DashBaseVelocity,1,10);
        ActionStatModel.OnAction = true;
    }
    

    public override void OnExit()
    {
        base.OnExit();
        OnAnimationEnd.Condition = false;
        MoveStatModel.RootMotionDisplacementMultor = 1;
        animationHandler.ApplyRootMotion = false;
        playerController.actorMovement.ApplyRootMotionYSpeed = false;
        timer.ClearAllTimers();
        ActionStatModel.OnAction = false;
    }

    public override void OnUpdate(float deltaTime)
    {
        OnAnimationEnd.Condition = director.Stage == ActionDirector.ActionDirectStage.End;
        base.OnUpdate(deltaTime);
    }
}