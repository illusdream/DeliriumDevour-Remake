using ilsActionEditor;
using StatModel;
using UnityEngine;

public class PlayerOnGroundAnimState : SubAFSMNode
{
    [SerializeReference]
    [ExitTranslation]
    public BaseFuncTranslation ToAir;

    private BaseActorMoveStatModel _actorMoveStatModel;
    public override void OnInit()
    {
        base.OnInit();
        _actorMoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
        ToAir.condition += Condition;
    }

    private bool Condition()
    {
        return !_actorMoveStatModel.OnGround;
    }
}