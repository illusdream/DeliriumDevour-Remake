using System;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class PlayerInputTranslation: BaseExitTranslation
{
    private PlayerInputHandler playerInputHandler;
    
    [ToggleLeft]
    [ShowInGraph]
    public bool TranslateByInput;
    
    [ShowInGraph]
    public EPlayerInput PlayerInput;
    
    [ShowInGraph]
    public EInputInteraction InputInteraction;
    
    [ShowInGraph]
    [MinMaxSlider(0,1)]
    public float InputBufferTime = 0f;
    
    public PlayerInputTranslation(Node node, string portName) : base(node, portName)
    {
    }
    
    public override void OnInitialize()
    {
        playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
        base.OnInitialize();
    }

    public override void StateEnter()
    {
        base.StateEnter();
    }

    public override bool CanTranslate()
    {
        if (TranslateByInput)
        {
            return playerInputHandler.HasTriggered(PlayerInput,InputInteraction,InputBufferTime);
        }
        else
        {
            return !playerInputHandler.HasTriggered(PlayerInput,InputInteraction,InputBufferTime);
        }
    }

    public override void StateExit()
    {
        base.StateExit();
    }
}