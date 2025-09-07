using System;
using ilsActionEditor;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

[Serializable]
public class PlayerMoveTranslation : BaseExitTranslation
{
    private PlayerInputHandler playerInputHandler;
    
    [ToggleLeft]
    [ShowInGraph]
    public bool TranslateByInput;
    
    public PlayerMoveTranslation(Node node, string portName) : base(node, portName)
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
            return playerInputHandler.Move.ActionValue != Vector2.zero;
        }
        else
        {
            return playerInputHandler.Move.ActionValue == Vector2.zero;
        }
        // return base.CanTranslate();
        
    }

    public override void StateExit()
    {
        base.StateExit();
    }
}