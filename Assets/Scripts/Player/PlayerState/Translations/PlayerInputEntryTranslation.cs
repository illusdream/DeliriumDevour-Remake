using System;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class PlayerInputEntryTranslation : BaseEntryTranslation
{
    public PlayerInputEntryTranslation(Node node, string portName) : base(node, portName)
    {
    }

    private PlayerInputHandler playerInputHandler;
    
    [ToggleLeft]
    [ShowInGraph]
    public bool TranslateByInput;
    
    [ShowInGraph]
    public EPlayerInput PlayerInput;

    [ShowInGraph]
    public float InputBufferTime = 0.5f;
    
    
    
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
        switch (PlayerInput)
        {
            case EPlayerInput.Move:
                if (TranslateByInput)
                {
                    return playerInputHandler.Move.ActionValue != Vector2.zero;
                }
                else
                {
                    return playerInputHandler.Move.ActionValue == Vector2.zero;
                }
                break;
            case EPlayerInput.Jump:
                if (TranslateByInput)
                {
                    return playerInputHandler.Move.ActionValue != Vector2.zero;
                }
                else
                {
                    return playerInputHandler.Move.ActionValue == Vector2.zero;
                }
                break;
            case EPlayerInput.Shift:
                if (TranslateByInput)
                {
                    return playerInputHandler.Shift._trackedAction.WasPerformedThisFrame();
                }
                else
                {
                    return !playerInputHandler.Shift._trackedAction.WasPerformedThisFrame();
                }
               
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // return base.CanTranslate();
        
    }

    public override void StateExit()
    {
        switch (PlayerInput)
        {
            case EPlayerInput.Move:
                break;
            case EPlayerInput.Jump:
                break;
            case EPlayerInput.Shift:
               
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        base.StateExit();
    }
}