using ilsFramework.Core;

namespace ilsActionEditor
{
    /// <summary>
    /// 应用于所有
    /// </summary>
    public interface ILogicDirectable
    {
        public static int LogicFrameCountPerSecond => Config.GetFrameworkConfig().LogicUpdateCountPerScecond;
        
        int startFrame { get; }
        

        int endFrame { get; }
        

        void LogicInitialize();


        void LogicEnter();


        void LogicUpdate(float time, float previousTime, int logicFrameCount);


        void LogicExit();
    }
}