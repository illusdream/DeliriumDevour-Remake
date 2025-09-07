using ilsFramework.Core;

namespace ilsActionEditor
{
    public interface ILogicDirectableTimePointer
    {
        IDirectable target { get; }
        
        ILogicDirectable logicTarget { get; }
        float time { get; }
        
        int frame { get; }
        void TriggerForward(float currentTime, float previousTime);
        
        void LogicUpdate(float currentTime, float previousTime);
    }

    public struct StartLogicTimePointer : ILogicDirectableTimePointer
    {
        private bool triggered;
        private float lastTargetStartTime;
        
        public IDirectable target{ get; private set; }
        public ILogicDirectable logicTarget{ get; private set; }
        public float time { get { return target.startTime; } }
        public int frame => logicTarget.startFrame;
        
        private int hasUpdatedLogicFrameIndex;

        public StartLogicTimePointer(IDirectable target, ILogicDirectable logicTarget)
        {
            triggered = false;
            this.target = target;
            this.logicTarget = logicTarget;
            lastTargetStartTime = target.startTime;
            hasUpdatedLogicFrameIndex = 0;
        }
        
        public void TriggerForward(float currentTime, float previousTime)
        {
            var cLogicFrame = target.GetLogicFrameCount(currentTime);
            
            if ( cLogicFrame >= logicTarget.startFrame ) {
                if ( !triggered ) {
                    triggered = true;
                    logicTarget.LogicEnter();
                    //确保第0帧会发生
                    logicTarget.LogicUpdate(target.ToLocalTime(currentTime), 0,0);
                }
            }
        }

        public void LogicUpdate(float currentTime, float previousTime)
        {
            if (target.root == null || logicTarget == null )
            {
                return;
            }
            //更新目标并尝试auto-key

            var deltaMoveClip = target.startTime - lastTargetStartTime;
            var localCurrentTime = target.ToLocalTime(currentTime);
            var localPreviousTime = target.ToLocalTime(previousTime + deltaMoveClip);
            
            var cLogicFrame = target.GetLogicFrameCount(currentTime);
            var timelineFinalLogicFrameIndex = target.GetLogicFrameCount(target.root.length);
            var cLocalLogicFrame = target.GetLogicFrameCount(localCurrentTime);

            if (cLogicFrame >= logicTarget.startFrame && cLogicFrame < logicTarget.endFrame && cLogicFrame > 0 && cLogicFrame < timelineFinalLogicFrameIndex)
            {
               
                while (hasUpdatedLogicFrameIndex < cLocalLogicFrame)
                {
                    //自动更新Key
#if UNITY_EDITOR
                    //if ( target is IKeyable && localCurrentTime == localPreviousTime ) {
                    // ( (IKeyable)target ).TryAutoKey(localCurrentTime);
                    // }
#endif
                    hasUpdatedLogicFrameIndex++;
                    logicTarget.LogicUpdate(localCurrentTime, localPreviousTime,hasUpdatedLogicFrameIndex);

                }
            }
        }
    }


    public struct EndLogicTimePointer : ILogicDirectableTimePointer
    {
        private bool triggered;
        
        public IDirectable target { get; private set; }
        public ILogicDirectable logicTarget { get; private set; }
        public float time { get { return target.endTime; } }
        public int frame => logicTarget.startFrame;
        
        public EndLogicTimePointer(IDirectable target, ILogicDirectable logicTarget) {
            this.target = target;
            this.logicTarget = logicTarget;
            triggered = false;
        }
        
        public void TriggerForward(float currentTime, float previousTime)
        {
            var endTime = target.endTime == target.parent.endTime ? target.endTime - 0.02f : target.endTime;
            if ( currentTime >= endTime) {
                if ( !triggered ) {
                    triggered = true;
                    //没有最后一帧了，直接退出
                    logicTarget.LogicExit();
                }
            }
        }

        public void LogicUpdate(float currentTime, float previousTime)
        {
            throw new System.NotImplementedException();
        }
    }
}