using Animancer;
using ilsActionEditor;
using ilsFramework.Core;
using StatModel;
using UnityEngine;

[NodeMenuItem("Player/InAir/JumpStartState")]
public class PlayerJumpStartState: BasePlayerState
{
        [SerializeReference]
        [EntryTranslation]
        public BaseEntryTranslation DefaultEntry;

        public override void OnInit()
        {
                MoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
                animationHandler = BlackBoard.GetValue<AnimationHandler>(AnimationHandler.BlackBoardKey);
                base.OnInit();
        }

        public override void OnEnter()
        {
                MoveStatModel.YSpeed.BaseValue = 8;
                base.OnEnter();
        }
}