using ilsFramework.Core;

namespace ilsActionEditor
{
    public class BaseConditionTranslation : BaseExitTranslation
    {
        public BaseConditionTranslation(XNode.Node node, string portName) : base(node, portName)
        {
        }

        public bool Condition;

        [ShowInGraph]
        public bool SetFalseWhenExit = true;
        public override bool CanTranslate()
        {
            return Condition;
        }

        public override void StateEnter()
        {
            base.StateEnter();
        }

        public override void StateExit()
        {
            if (SetFalseWhenExit)
            {
                Condition = false;
            }
            base.StateExit();
        }
    }
}