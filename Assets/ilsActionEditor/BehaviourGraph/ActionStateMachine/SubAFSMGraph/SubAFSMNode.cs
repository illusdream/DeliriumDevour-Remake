using System;
using System.Collections.Generic;
using System.Linq;
using ilsFramework.Core;
using UnityEngine;

namespace ilsActionEditor
{
    public class SubAFSMNode : AFSMStateNode
    {
        [ShowInGraph]
        public SubAFSMGraph subFSMGraph;
        private AFSMTranslationMode translationMode;

        [SerializeReference]
        [ExitTranslation]
        public SubGraphDefaultTranslation defaultExit;

        private Dictionary<string, SubGraphDefaultTranslation> allGraphExits;
        
        public override void OnInit()
        {
            if (!subFSMGraph)
            {
                return;
            }
            subFSMGraph.Initialize(BlackBoard);
            subFSMGraph.AFSMTranslationMode = (graph as AFSMGraph).AFSMTranslationMode;
            translationMode = subFSMGraph.AFSMTranslationMode;
            
            InitAllGraphExits();
            
            base.OnInit();
        }

        private void InitAllGraphExits()
        {
            allGraphExits = new Dictionary<string, SubGraphDefaultTranslation>();
            foreach (var fieldExit in fieldExitTranslations)
            {
                if (fieldExit is SubGraphDefaultTranslation exit)
                {
                    allGraphExits.Add(exit.ExitKey, exit);
                }
            }

            foreach (var exit in Exits)
            {
                if (exit is SubGraphDefaultTranslation _exit)
                {
                    allGraphExits.Add(_exit.ExitKey, _exit);
                }
            }
        }

        public override void OnEnter()
        {
            if (!subFSMGraph)
            {
                return;
            }
            subFSMGraph.StartFSM();
            base.OnEnter();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!subFSMGraph)
            {
                return;
            }

            if (translationMode == AFSMTranslationMode.Update)
            {
                CheckInnerAFSMGraphExit();
            }
            subFSMGraph.DoUpdate(deltaTime);
            base.OnUpdate(deltaTime);
        }

        public override void OnLogicUpdate()
        {
            if (!subFSMGraph)
            {
                return;
            }
            if (translationMode == AFSMTranslationMode.LogicUpdate)
            {
                CheckInnerAFSMGraphExit();
            }
            subFSMGraph.LogicUpdate();
            base.OnLogicUpdate();
        }

        public override void OnFixedUpdate()
        {
            if (!subFSMGraph)
            {
                return;
            }
            if (translationMode == AFSMTranslationMode.FixedUpdate)
            {
                CheckInnerAFSMGraphExit();
            }
            subFSMGraph.FixedUpdate();
            base.OnFixedUpdate();
        }

        public override void OnLateUpdate()
        {
            if (!subFSMGraph)
            {
                return;
            }
            if (translationMode == AFSMTranslationMode.LateUpdate)
            {
                CheckInnerAFSMGraphExit();
            }
            subFSMGraph.LateUpdate();
            base.OnLateUpdate();
        }

        public override void OnExit()
        {
            if (!subFSMGraph)
            {
                return;
            }
            subFSMGraph.EndFSM();
            base.OnExit();
        }

        public override void OnStateDestroy()
        {
            if (!subFSMGraph)
            {
                return;
            }
            subFSMGraph.OnDestroy();
            base.OnStateDestroy();
        }

        private void CheckInnerAFSMGraphExit()
        {
            var exitKey = subFSMGraph.CurrentExitKey();
            if (allGraphExits.TryGetValue(exitKey,out var value))
            {
                value.WillExit = true;
            }
        }

        public HashSet<string> GetAllExitNodeKeys()
        {
            if (subFSMGraph != null)
            {
                return subFSMGraph.GetAllExitNodeKeys();
            }
            HashSet<string> keys = new HashSet<string>(){ SubAFSMGraph.NULLExitStringKey};
            return keys;
        }

        public void OnCopy()
        {
            subFSMGraph = subFSMGraph.Copy() as SubAFSMGraph;
        }
        
        public class SubGraphDefaultTranslation : BaseExitTranslation
        {
            public SubGraphDefaultTranslation(XNode.Node node, string portName) : base(node, portName)
            {
            }
            public string ExitKey = SubAFSMGraph.NULLExitStringKey;
            
            public bool WillExit = false;

            public override bool CanTranslate()
            {
                return WillExit;
            }

            public override void StateEnter()
            {
                WillExit = false;
                base.StateEnter();
            }

            public override void StateExit()
            {
                WillExit = false;
                base.StateExit();
            }
        }
    }
}