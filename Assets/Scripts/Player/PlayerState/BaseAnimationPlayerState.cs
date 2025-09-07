using Animancer;
using ilsActionEditor;
using UnityEngine;
using UnityEngine.Serialization;

[NodeMenuItem("Player/BaseAnimationPlayerState")]
public class BaseAnimationPlayerState : AFSMStateNode
{
    [FormerlySerializedAs("EndAnimationEnd")]
    [ExitTranslation]
    [SerializeReference]
    public BaseConditionTranslation OnAnimationEnd;

    [ShowInGraph]
    public ClipTransition Animation;
    
    private PlayerInputHandler playerInputHandler;
    private AnimancerComponent animancerComponent;
    private AnimationHandler animHandler;
    private Transform MainTransform;
    private PlayerController controller;


    public float TestValue;
    public override void OnInit()
    {
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        animHandler = BlackBoard.GetValue<AnimationHandler>(AnimationHandler.BlackBoardKey);
        animancerComponent = animHandler.animancerComponent;
        MainTransform = BlackBoard.GetValue<Transform>("MainTransform");
        controller = BlackBoard.GetValue<PlayerController>("PlayerController");
        base.OnInit();
    }

    private void OnEnd()
    {
        OnAnimationEnd.Condition = true;
        Animation.Events.OnEnd = null;
    }   
    
    public override void OnEnter()
    {
        Animation.Events.OnEnd = OnEnd;
        animancerComponent.Play(Animation);
        
        
        
        base.OnEnter();
    }

    public override void OnUpdate(float deltaTime)
    {
        
        
        controller.UpdateMovement(playerInputHandler.Move.ActionValue);
        base.OnUpdate(deltaTime);
    }
}