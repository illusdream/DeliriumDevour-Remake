using System;

namespace ilsActionEditor
{
    public class BaseFuncTranslation : BaseExitTranslation
    {
        public Func<bool> condition;
        
        public BaseFuncTranslation(XNode.Node node, string portName) : base(node, portName)
        {
        }

        public override bool CanTranslate()
        {
            return condition != null && condition();
        }
    }
}