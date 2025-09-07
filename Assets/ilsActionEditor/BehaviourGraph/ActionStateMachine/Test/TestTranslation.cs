using System;
using Sirenix.OdinInspector;

namespace ilsActionEditor.Test
{
    [Serializable]
    public class TestTranslation : AFSMStateTranslation
    {
        [ShowInInspector]
        public float awaitTime;
        private float _awaitTime;
        public TestTranslation(XNode.Node node, string portName) : base(node, portName)
        {
        }

        public override void Update(float deltaTime)
        {
            if (_awaitTime >0)
            {
                _awaitTime -= deltaTime;
            }
            base.Update(deltaTime);
        }

        public override void StateEnter()
        {
            _awaitTime = awaitTime;
            base.StateEnter();
        }

        public override bool CanTranslate()
        {
            if (_awaitTime <= 0)
            {
                _awaitTime = awaitTime;
                return true;
            }

            return false;
        }
    }
}