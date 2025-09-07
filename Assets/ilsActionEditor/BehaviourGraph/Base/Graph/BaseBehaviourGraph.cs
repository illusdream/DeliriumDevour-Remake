using System;
using System.Collections.Generic;
using ilsFramework.Core;
using UnityEngine;
using XNode;

namespace ilsActionEditor
{
    [CreateAssetMenu(fileName = "BehaviourGraph", menuName = "ilsActionEditor/BehaviourGraph")]
    public abstract class BaseBehaviourGraph : XNode.NodeGraph
    {
        //保存key以及对应Type的类
        
        
        private BlackBoard _blackBoard;

        public BlackBoard BlackBoard
        {
            get=>_blackBoard;
            set=>_blackBoard=value;
        }
        public abstract void Initialize(BlackBoard blackBoard);
        
        public abstract void DoUpdate(float deltaTime);

        public abstract void LogicUpdate();
        
        public abstract void FixedUpdate();
        
        public abstract void LateUpdate();

        public override NodeGraph Copy()
        {
#if UNITY_EDITOR
            var copy = base.Copy();
            var copyfinal = copy as BaseBehaviourGraph;
            copyfinal.CopyFrom = this;
            return copy;
#else
            return base.Copy();
#endif
        }

#if UNITY_EDITOR
        public BaseBehaviourGraph CopyFrom;
#endif
    }
}