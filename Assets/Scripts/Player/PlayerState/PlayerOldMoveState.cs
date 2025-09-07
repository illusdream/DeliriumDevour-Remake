using System;
using Animancer;
using DefaultNamespace;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class PlayerOldMoveState : BaseActionPlayerState
{
    private PlayerController controller;
    
    private AnimationHandler animHandler;
    private PlayerInputHandler playerInputHandler;
    [ExitTranslation]
    [SerializeReference]
    public PlayerMoveTranslation MoveToIdle;

    public StringAsset MoveX;
    public StringAsset MoveY;
    private SmoothedVector2Parameter _SmoothedParameters;

    public StringAsset Speed;
    private SmoothedFloatParameter _SmoothedFloatParameters;
    
    public float smoothTime = 0.2f;
    public float smoothTime2 =1f;


    public override void OnInit()
    {
        controller = BlackBoard.GetValue<PlayerController>("PlayerController");
        animHandler = BlackBoard.GetValue<AnimationHandler>(AnimationHandler.BlackBoardKey);
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");

        base.OnInit();
    }

    public override void OnEnter()
    {

        animHandler.RootMotionApply += RootMotionApply;
        base.OnEnter();
    }

    private void RootMotionApply(Animator animator,Vector3 arg1, Quaternion arg2)
    {
        controller.transform.position += arg1;
    }

    public override void OnUpdate(float deltaTime)
    {

       var Shift = playerInputHandler.Shift._trackedAction.IsPressed();
           // Calculate the movement direction.


       //playerInputHandler.Shift.ResetTriggers();
        base.OnUpdate(deltaTime);
    }

    public override void OnLogicUpdate()
    {
        base.OnLogicUpdate();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
    }

    public override void OnExit()
    {
        _SmoothedParameters.TargetValue = new Vector2(0, 0);
        var Shift = playerInputHandler.Shift._trackedAction.IsPressed();
        _SmoothedFloatParameters.TargetValue = Shift ? 1 : 0;
        animHandler.RootMotionApply -= RootMotionApply;
        base.OnExit();
    }
    
    
}
