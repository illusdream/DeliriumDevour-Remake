using ilsActionEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerOnActionOverrideState : BasePlayerAnimState
    {
        [ExitTranslation]
        [SerializeReference]
        public BaseFuncTranslation NoInAction;

        public override void OnInit()
        {
            base.OnInit();
            NoInAction.condition += Condition;
        }

        private bool Condition()
        {
            return !ActionStatModel.OnAction;
        }
    }
}