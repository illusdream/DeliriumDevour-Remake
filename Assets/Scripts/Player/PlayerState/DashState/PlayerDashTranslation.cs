using ilsActionEditor;
using XNode;

public class PlayerDashTranslation : BaseExitTranslation
{
    private PlayerInputHandler playerInputHandler;
    
    private OnGroundSensor groundSensor;
    
    public PlayerDashTranslation(Node node, string portName) : base(node, portName)
    {
    }

    public override void OnInitialize()
    {
        
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        groundSensor = BlackBoard.GetValue<OnGroundSensor>(OnGroundSensor.BlackBoardKey);
        base.OnInitialize();
    }

    public override bool CanTranslate()
    {
        return playerInputHandler.HasTriggered(EPlayerInput.Shift, EInputInteraction.Press, 0.02f) && groundSensor.CoyoteBuffer;
    }
}