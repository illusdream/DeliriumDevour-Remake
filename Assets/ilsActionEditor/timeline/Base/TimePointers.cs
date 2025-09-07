using ilsFramework.Core;
using Sirenix.OdinInspector;
using Slate;

namespace ilsActionEditor
{

    ///<summary>时间点的接口</summary>
    public interface IDirectableTimePointer
    {
        IDirectable target { get; }
        float time { get; }
        void TriggerForward(float currentTime, float previousTime);
        void TriggerBackward(float currentTime, float previousTime);
        void Update(float currentTime, float previousTime);
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>记录group、track或clip（IDirectable）的startTime及其相关执行</summary>
    public struct StartTimePointer : IDirectableTimePointer
    {
        
        private bool triggered;
        private float lastTargetStartTime;
        [ShowInInspector]
        public IDirectable target { get; private set; }
        float IDirectableTimePointer.time { get { return target.startTime; } }
        
        private ILogicDirectable logic;

        private int hasUpdatedLogicFrameIndex;
        public StartTimePointer(IDirectable target) {
            this.target = target;
            triggered = false;
            lastTargetStartTime = target.startTime;
            logic = target as ILogicDirectable;
            hasUpdatedLogicFrameIndex = -1;
        }

        //...
        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime) {
            if ( currentTime >= target.startTime ) {
                if ( !triggered ) {
                    triggered = true;
                    target.Enter();
                    target.Update();
                }
            }
        }

        //...
        void IDirectableTimePointer.Update(float currentTime, float previousTime) {

            if (target.root == null)
            {
                return;
            }
            //更新目标并尝试auto-key
            if ( currentTime >= target.startTime && currentTime < target.endTime && currentTime > 0 && currentTime < target.root.length ) {

                var deltaMoveClip = target.startTime - lastTargetStartTime;
                var localCurrentTime = target.ToLocalTime(currentTime);
                var localPreviousTime = target.ToLocalTime(previousTime + deltaMoveClip);

#if UNITY_EDITOR
                //if ( target is IKeyable && localCurrentTime == localPreviousTime ) {
                   // ( (IKeyable)target ).TryAutoKey(localCurrentTime);
               // }
#endif

                target.Update();
                lastTargetStartTime = target.startTime;
            }

            //root updated callback
            if ( currentTime > 0 ) {
                target.RootUpdated(currentTime, previousTime);
            }
        }
        
        //...
        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime) {
            if ( currentTime < target.startTime || currentTime <= 0 ) {
                if ( triggered ) {
                    triggered = false;
                    target.Update();
                    target.Reverse();
                }
            }
        }
    }

    ///----------------------------------------------------------------------------------------------

    ///<summary>将group、track或clip（IDirectable）的endTime及其相关执行打包</summary>
    public struct EndTimePointer : IDirectableTimePointer
    {

        private bool triggered;
        public IDirectable target { get; private set; }
        float IDirectableTimePointer.time { get { return target.endTime; } }

        public EndTimePointer(IDirectable target) {
            this.target = target;
            triggered = false;
        }

        //...
        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime) {
            if ( currentTime >= target.endTime ) {
                if ( !triggered ) {
                    triggered = true;
                    target.Update();
                    target.Exit();
                }
            }
        }

        //...
        void IDirectableTimePointer.Update(float currentTime, float previousTime) {
            //Update is/should never be called in TimeOutPointers
            throw new System.NotImplementedException();
        }

        public void LogicUpdate(float currentTime, float previousTime)
        {
            throw new System.NotImplementedException();
        }

        //...
        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime) {
            if ( currentTime < target.endTime ) {
                if ( triggered ) {
                    triggered = false;
                    target.ReverseEnter();
                    target.Update();
                }
            }
        }
    }
}