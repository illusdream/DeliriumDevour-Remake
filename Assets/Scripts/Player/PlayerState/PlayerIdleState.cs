using System;
using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using StatModel;
using UnityEngine;
using XNode;

public class PlayerIdleState : BasePlayerState
{
    [ExitTranslation]
    [SerializeReference]
    public PlayerIdleToMove IdleToMove;
    
    private ClipTransition CurrentAnim;
    
    
    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnEnter()
    {
        ChooseCurrentAnim();
       // animationHandler.Play(CurrentAnim);
       // animationHandler.ApplyRootMotion = true;
       // playerController.actorMovement.ApplyRootMotionYSpeed = false;
        MoveStatModel.Speed.BaseValue = 0;
        base.OnEnter();
    }
    

    public override void OnUpdate(float deltaTime)
    {
        //MoveStatModel.Speed.BaseValue = Mathf.Lerp(MoveStatModel.Speed.BaseValue,0,0.2f);
        playerController.UpdateMovement(playerInputHandler.Move.ActionValue);
        base.OnUpdate(deltaTime);
    }

    private void ChooseCurrentAnim()
    {
        //判断是左脚还是右脚
        Transform leftFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = animationHandler.animator.GetBoneTransform(HumanBodyBones.RightFoot);

        Vector3 leftFootLocalPos = playerTransform.InverseTransformPoint(leftFoot.position);
        Vector3 rightFootLocalPos = playerTransform.InverseTransformPoint(rightFoot.position);
        
        CurrentAnim = leftFootLocalPos.z > rightFootLocalPos.z ? playerController.playerBaseStat.Idle_Left_Anim : playerController.playerBaseStat.Idle_Right_Anim;
    }

    public override void OnExit()
    {
        animationHandler.ApplyRootMotion = false;
        base.OnExit();
    }
}
[Serializable]
public class PlayerIdleToMove : BaseExitTranslation
{
    private PlayerInputHandler playerInputHandler;
    private BaseActorMoveStatModel ActorMoveStatModel;
    public PlayerIdleToMove(Node node, string portName) : base(node, portName)
    {
    }

    public override void OnInitialize()
    {
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        ActorMoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
        base.OnInitialize();
    }

    public override void StateEnter()
    {
        base.StateEnter();
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    private void OnActionComplete()
    {
    }

    public override bool CanTranslate()
    {
        return playerInputHandler.Move.ActionValue != Vector2.zero || ActorMoveStatModel.Speed.Value < 0.001f;
    }
}