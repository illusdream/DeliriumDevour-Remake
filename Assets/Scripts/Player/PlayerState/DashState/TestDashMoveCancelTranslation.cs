using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using StatModel;
using UnityEngine;
using XNode;

public class TestDashMoveCancelTranslation : BaseExitTranslation
{
    private BaseActorActionStatModel actionStatModel;
    
    private PlayerInputHandler playerInputHandler;

    public StringAsset targetCancel;
    
    public TestDashMoveCancelTranslation(Node node, string portName) : base(node, portName)
    {
    }

    public override void OnInitialize()
    {
        actionStatModel = BlackBoard.GetValue<BaseActorActionStatModel>(BaseActorActionStatModel.BlackBoardKey);
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        base.OnInitialize();
    }

    public override bool CanTranslate()
    {
        return actionStatModel.RecoveryCancels.Contains(targetCancel.Name) && playerInputHandler.Move.ActionValue != Vector2.zero ;
        return base.CanTranslate();
    }
}