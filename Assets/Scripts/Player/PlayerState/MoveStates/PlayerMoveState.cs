using System;
using ilsActionEditor;
using ilsFramework.Core;
using UnityEngine;
using XNode;

[NodeMenuItem("Player/Move/PlayerMoveState")]
public class PlayerMoveState : SubAFSMNode
{
    [ExitTranslation]
    [SerializeReference]
    private ToIdleTranslation MoveToIdle;
    

    public override void OnInit()
    {
        base.OnInit();
    }

    private class ToIdleTranslation : PlayerMoveTranslation
    {
        public ToIdleTranslation(Node node, string portName) : base(node, portName)
        {
        }

        public override bool CanTranslate()
        {
           // (base.CanTranslate() && ((State as PlayerMoveState)?.subFSMGraph.IsInExitNode()).GetValueOrDefault(false)).LogSelf();
            return base.CanTranslate() &&( (State as PlayerMoveState)?.subFSMGraph.IsInExitNode()).GetValueOrDefault(false);
        }
    }
}