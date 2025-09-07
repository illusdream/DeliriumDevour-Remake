using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using StatModel;
using UnityEngine;

[NodeMenuItem("Player/Move/EndState")]
public class PlayerMove_EndState : BasePlayerState
{
    [ExitTranslation]
    [SerializeReference]
    public BaseFuncTranslation OnEndState;

    [ExitTranslation]
    [SerializeReference]
    public PlayerMoveTranslation OnReMove;
    
    private bool _idle;
    public override void OnInit()
    {
        OnEndState.condition  += Condition;
        base.OnInit();
    }

    public override void OnEnter()
    {
        _idle = false;
        base.OnEnter();
    }
    
    private bool Condition()
    {
        if (!_idle) return _idle;
        _idle = false;
        return true;
    }

    public override void OnUpdate(float deltaTime)
    {        
        playerController.UpdateMovement(playerInputHandler.Move.ActionValue);
        base.OnUpdate(deltaTime);
    }

    public override void OnLogicUpdate()
    {
        var velocity = MoveStatModel.HorizontalNormalizedVelocity * MoveStatModel.Speed.BaseValue;
        if (velocity.magnitude <(playerController.playerBaseStat.Acc * Time.fixedDeltaTime))
        {
            velocity = Vector2.zero;
        }
        else
        {
            velocity -=MoveStatModel.HorizontalNormalizedVelocity * (playerController.playerBaseStat.Acc * Time.fixedDeltaTime);
            velocity =Vector2.ClampMagnitude(velocity, playerController.playerBaseStat.BaseMoveSpeed);
        }
        MoveStatModel.SetBaseVelocity(new Vector3(velocity.x,0,velocity.y),false);
        base.OnLogicUpdate();
    }
}