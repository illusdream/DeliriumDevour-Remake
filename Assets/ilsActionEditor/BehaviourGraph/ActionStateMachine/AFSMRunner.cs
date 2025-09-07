using System;
using System.Collections.Generic;
using System.Diagnostics;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ilsActionEditor
{
    public class AFSMRunner : MonoBehaviour,IHostedLogicUpdate
    {
        [SerializeField]
        private AFSMTranslationMode _AFSMTranslationMode;
        
        public AFSMTranslationMode AFSMTranslationMode
        {
            get => _AFSMTranslationMode;
            set => _AFSMTranslationMode = value;
        }
        
        public AFSMGraph FSMGraph;
        
        [ShowInInspector]
        private AFSMGraph currentRunTimeGraph;
        
        public List<ActionGraphTuple> actionGraphTuples = new List<ActionGraphTuple>();
        
        public MonoBlackBoard blackBoard;
        public void Start()
        {
            currentRunTimeGraph = FSMGraph.Copy() as AFSMGraph;
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.Initialize(blackBoard.blackBoard);
                currentRunTimeGraph.StartFSM();
                currentRunTimeGraph.AFSMTranslationMode = AFSMTranslationMode;
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.Initialize(AFSMTranslationMode,blackBoard.blackBoard);
            });
           
        }

        public void OnEnable()
        {
            InputManager.Instance.RegisterHostedUpdate(this);
        }

        public void Update()
        {
            //先更新最底下的Locomotion的
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.DoUpdate(Time.deltaTime);
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.DoUpdate(Time.deltaTime);
            });
        }

        public void FixedUpdate()
        {
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.FixedUpdate();
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.DoFixedUpdate();
            });
        }

        public void LateUpdate()
        {
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.LateUpdate();
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.DoLateUpdate();
            });
        }

        public void OnDestroy()
        {
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.OnDestroy();
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.OnDestroy();
            });
        }

        public void HostedLogicUpdate()
        {
            if (currentRunTimeGraph)
            {
                currentRunTimeGraph.LogicUpdate();
            }
            actionGraphTuples.ForEach((actionGraphTuple) =>
            {
                actionGraphTuple.DoLogicUpdate();
            });
        }

        public void OnDisable()
        {
            InputManager.Instance?.RegisterHostedUpdate(this);
        }
    }
}