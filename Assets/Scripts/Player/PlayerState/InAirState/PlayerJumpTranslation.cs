using ilsActionEditor;
using StatModel;
using XNode;

public class PlayerJumpTranslation : BaseExitTranslation
{
    public PlayerJumpTranslation(Node node, string portName) : base(node, portName)
    {
    }
    
    private PlayerInputHandler playerInputHandler;

    private BaseActorMoveStatModel actorMoveStatModel;
    
    private OnGroundSensor groundSensor;
    
    public override void OnInitialize()
    {
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        actorMoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
        groundSensor = BlackBoard.GetValue<OnGroundSensor>(OnGroundSensor.BlackBoardKey);
        base.OnInitialize();
    }

    public override bool CanTranslate()
    {
        return playerInputHandler.HasTriggered(EPlayerInput.Jump, EInputInteraction.Tap, 0.02f) && groundSensor.OnGround;
    }
}