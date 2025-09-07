using System;
using System.Collections.Generic;
using ilsFramework.Core;
using UnityEngine;
using XNode;

namespace ilsActionEditor
{
    [CreateAssetMenu(fileName = "FSMGraph", menuName = "ilsActionEditor/FSMGraph")]
    public class AFSMGraph : BaseBehaviourGraph
    {
        
        private AFSMStateNode _currentState;
        
        public AFSMStateNode CurrentState => _currentState;
        
        [SerializeReference]
        private AFSMStateNode _entryState;
        
        public AFSMStateNode EntryState => _entryState;
        
        public List<AFSMGlobalState> GlobalStates;
        
        [SerializeField]
        private AFSMTranslationMode _AFSMTranslationMode;
        
        public AFSMTranslationMode AFSMTranslationMode
        {
            get => _AFSMTranslationMode;
            set => _AFSMTranslationMode = value;
        }
        
        public Dictionary<string,AFSMStateNode> NameStateDictionary;
        
        
        public void SetEntryState(AFSMStateNode entryState)
        {
            _entryState = entryState;
        }


        public override void Initialize(BlackBoard blackBoard)
        {
            BlackBoard = blackBoard;
            GlobalStates = new List<AFSMGlobalState>();
            NameStateDictionary = new Dictionary<string, AFSMStateNode>();
            foreach (var node in nodes)
            {
                if (node is AFSMStateNode afsmNode)
                {
                    afsmNode.Initialize();
                    NameStateDictionary.TryAdd(afsmNode.Name, afsmNode);
                }

                if (node is AFSMGlobalState globalState)
                {
                    GlobalStates.Add(globalState);
                }
            }
            GlobalStates.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public virtual void StartFSM()
        {
            _currentState = _entryState;
            _currentState.Enter();
        }

        public override void DoUpdate(float deltaTime)
        {
            if (AFSMTranslationMode is AFSMTranslationMode.Update)
            {
                CheckTransition(deltaTime);
            }
            _currentState?.DoUpdate(deltaTime);
        }

        public override void LogicUpdate()
        {
            if (AFSMTranslationMode is AFSMTranslationMode.LogicUpdate)
            {
                CheckTransition(1/50f);
            }
            _currentState?.LogicUpdate();
        }

        public override void FixedUpdate()
        {
            if (AFSMTranslationMode is AFSMTranslationMode.FixedUpdate)
            {
                CheckTransition(Time.fixedDeltaTime);
            }
            _currentState?.FixedUpdate();
        }

        public override void LateUpdate()
        {
            if (AFSMTranslationMode is AFSMTranslationMode.LateUpdate)
            {
                CheckTransition(Time.deltaTime);
            }
            _currentState?.LateUpdate();
        }

        public virtual void EndFSM()
        {
            _currentState?.Exit();
            _currentState = null;
        }
        
        public void OnDestroy()
        {
            EndFSM();
            _currentState = null;
            foreach (var node in nodes)
            {
                if (node is AFSMStateNode stateNode)
                {
                    stateNode.Destroy();
                }
            }
        }

        public void CheckTransition(float deltaTime)
        {
            foreach (var globalState in GlobalStates)
            {
                var globalNextNode = globalState.GetTransition(deltaTime);
                if (globalNextNode)
                {
                    _currentState.Exit();
                    _currentState = globalNextNode;
                    _currentState.Enter();
                    return;
                }
            }
            
            
            if (_currentState == null)
            {
                return;
            }

            var nextNode = _currentState.GetTransition(deltaTime);
            if (nextNode)
            {
                _currentState.Exit();
                _currentState = nextNode;
                _currentState.Enter();
            }
        }

        public void ChangeState(AFSMStateNode nextState)
        {
            _currentState.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void ChangeState(string nextStateName)
        {
            _currentState.Exit();
            
            _currentState.Enter();
        }

        public override NodeGraph Copy()
        {
            var result = base.Copy() as AFSMGraph;
            result._entryState = result.nodes[nodes.IndexOf(_entryState)] as AFSMStateNode;
            foreach (var node in result.nodes)
            {
                if (node is SubAFSMNode subFSMNode)
                {
                    subFSMNode.OnCopy();
                }
            }
            return result;
        }
    }
}