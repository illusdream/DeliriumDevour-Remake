namespace ilsActionEditor
{
    public enum ETimePointState
    {
        //还未触发
        NonTriggered,
        //正在运行中
        Execution,
        //超出这个时间
        Triggered
    }
    
    public class TimelineTimePoint
    {
        IDirectable target { get; }

        private int previousFrameIndex;
        
        private ETimePointState _state;

        public ETimePointState State
        {
            get { return _state; }

            set
            {
                //NonoTiggered-》Execution 触发Enter函数和Update 
                //Execution -》 Tiggered 触发Update（最后一次）和 Exit
                if (_state is ETimePointState.NonTriggered && value is ETimePointState.Execution)
                {
                    target.Enter();
                }
                if (_state is ETimePointState.Execution && value is ETimePointState.Triggered)
                {
                    target.Exit();
                }
                _state = value;
            }
        }

        public TimelineTimePoint(IDirectable target)
        {
            this.target = target;
            _state = ETimePointState.NonTriggered;
            previousFrameIndex = 0;
        }
        

        public void Update()
        {
            if (State is ETimePointState.Execution)
            {
                target.Update();
            }
        }

        public void LogicUpdate(int frameIndex)
        {
            if (frameIndex < target.startFrame)
            {
                State = ETimePointState.NonTriggered;
            }
            if (frameIndex >= target.startFrame && frameIndex < target.endFrame)
            {
                State = ETimePointState.Execution;
            }
            if (frameIndex >= target.endFrame)
            {
                State = ETimePointState.Triggered;
            }
            previousFrameIndex = frameIndex;

            if (State is ETimePointState.Execution)
            {
                target.LogicUpdate();
            }
        }

        public void OnInterruptTimeline()
        {
            if (State is ETimePointState.Execution)
            {
                State = ETimePointState.Triggered;
            }
        }
    }
}