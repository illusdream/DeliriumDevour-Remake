using ilsActionEditor;
using StatModel;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerAnimGlobalState : AFSMGlobalState
    {
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation InAction;

        private BaseActorActionStatModel ActionStatModel;
        public override void OnInit()
        {
            base.OnInit();
            ActionStatModel = BlackBoard.GetValue<BaseActorActionStatModel>(BaseActorActionStatModel.BlackBoardKey);
            InAction.condition += Condition;
        }

        private bool Condition()
        {
            return ActionStatModel.OnAction;
        }
    }
}