using ilsActionEditor.Test.Clip;

namespace ilsActionEditor.Test.Track
{
    [TargetClip(typeof(TestClip))]
    public class TestTrack : BaseActionTrack,ILogicDirectable
    {
        public int startFrame { get; }
        public int endFrame { get; }
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
    }
}