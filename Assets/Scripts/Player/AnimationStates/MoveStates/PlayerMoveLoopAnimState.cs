using ilsActionEditor;
using UnityEngine;

[NodeMenuItem("Player/Animation/Move/Loop")]
public class PlayerMoveLoopAnimState : BasePlayerAnimState
{
    [ExitTranslation]
    [SerializeReference]
    public BaseFuncTranslation ToStop;

    public override void OnInit()
    {
        base.OnInit();
        ToStop.condition += Condition;
    }

    private bool Condition()
    {
        return Mathf.Approximately(MoveStatModel.Speed.GetValue(), 0);
    }

    public override void OnEnter()
    {
        animationHandler.Play(animationHandler.actorLocomotionSets.Move_Loop_Anim);
        base.OnEnter();
    }
}