using System.Collections.Generic;
using UnityEngine;

namespace ilsActionEditor
{

    
    ///<summary>IDirector的所有可执行元素（组、轨迹、剪辑等）的接口</summary>
    public interface IDirectable
    {

        IDirector root { get; }
        IDirectable parent { get; }
        IEnumerable<IDirectable> children { get; }

        GameObject actor { get; }
        string name { get; }
        bool isActive { get; }
        bool isCollapsed { get; }
        bool isLocked { get; }

        float startTime { get; }
        float endTime { get; }
        
        int startFrame { get; }
        
        int endFrame { get; }
        float blendIn { get; }
        float blendOut { get; }
        bool canCrossBlend { get; }

        void Validate(IDirector root, IDirectable parent);
        bool Initialize();
        void Enter();
        void Exit();
        void Update();
        void ReverseEnter();
        void Reverse();
        
        void LogicUpdate();
        
        void RootEnabled();
        void RootUpdated(float time, float previousTime);
        void RootDisabled();
        void RootDestroyed();

#if UNITY_EDITOR
        void DrawGizmos(bool selected);
        void SceneGUI(bool selected);
#endif

    }

    ///----------------------------------------------------------------------------------------------
    
    //可设置关键帧参数的
    ///<summary>For Directables that contain keyable parameters.</summary>
    public interface IKeyable : IDirectable
    {
        AnimationDataCollection animationData { get; }
        object animatedParametersTarget { get; }
    }

    ///<summary>用于包装内容的Directables，如动画/音频剪辑。（暂时认为是给其他剪辑的额外设置）</summary>
    public interface ISubClipContainable : IDirectable
    {
        float subClipOffset { get; set; }
        float subClipSpeed { get; }
        float subClipLength { get; }
    }
}