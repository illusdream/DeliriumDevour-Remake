using System;
using System.Linq;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    [TargetClip(typeof(ActionPhase_StartUpClip),typeof(ActionPhase_ExecutionClip),typeof(ActionPhase_RecoveryClip))]
    [UniqueElement]
    public class ActionPhasesTrack  : BaseActionTrack,ILogicDirectable
    {
        [ShowInInspector]
        public ActionPhase_StartUpClip StartUpClip { get; private set; }
        [ShowInInspector]
        public ActionPhase_ExecutionClip ExecutionClip { get; private set; }
        [ShowInInspector]
        public ActionPhase_RecoveryClip RecoveryClip { get; private set; }
        //开始添加三个阶段的数据


        public int startFrame=>Mathf.CeilToInt( startTime * ILogicDirectable.LogicFrameCountPerSecond);
        public int endFrame =>Mathf.CeilToInt( endTime * ILogicDirectable.LogicFrameCountPerSecond);
        public void LogicInitialize()
        {
            
        }

        public void LogicEnter()
        {
            
        }

        public void LogicUpdate(float time, float previousTime, int logicFrameCount)
        {
   
        }

        public void LogicExit()
        {
           
        }
#if UNITY_EDITOR
        protected override void OnCreate()
        {
            int timeFrame =endFrame;
            var perClipLength = timeFrame / 3;
            float counter =0;
            StartUpClip = AddAction<ActionPhase_StartUpClip>(counter / ILogicDirectable.LogicFrameCountPerSecond);
            StartUpClip.endTime = (float)perClipLength / ILogicDirectable.LogicFrameCountPerSecond;
            counter = StartUpClip.endTime;
            
            ExecutionClip = AddAction<ActionPhase_ExecutionClip>(counter);
            ExecutionClip.endTime =counter+ (float)perClipLength / ILogicDirectable.LogicFrameCountPerSecond;
            counter = ExecutionClip.endTime;
            
            RecoveryClip = AddAction<ActionPhase_RecoveryClip>(counter);
            RecoveryClip.endTime = endTime;
            base.OnCreate();
        }

        public void OnValidate()
        {
            if (!StartUpClip)
            {
                StartUpClip = clips.FirstOrDefault(c => c is ActionPhase_StartUpClip) as ActionPhase_StartUpClip;
            }

            if (!ExecutionClip)
            {
                ExecutionClip = clips.FirstOrDefault(c => c is ActionPhase_ExecutionClip) as ActionPhase_ExecutionClip;
            }

            if (!RecoveryClip)
            {
                RecoveryClip = clips.FirstOrDefault(c => c is ActionPhase_RecoveryClip) as ActionPhase_RecoveryClip;
            }
        }
#endif
    }
}