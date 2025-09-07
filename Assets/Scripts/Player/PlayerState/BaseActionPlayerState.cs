using ilsActionEditor;
using Sirenix.OdinInspector;
using UnityEngine;

[NodeMenuItem("Player/BaseActionPlayerState")]
public abstract class BaseActionPlayerState : BasePlayerState
{
    [ShowInGraph]
    public ActionTimeline timeline;
    [FoldoutGroup("外部组件/组件/功能")]
    public ActionDirector director;
        
    [ShowInGraph]
    public TimelineWarpMode WarpMode;
        

    private int frameIndex;
    public override void OnInit()
    {
        base.OnInit();
        director = BlackBoard.GetValue<ActionDirector>(ActionDirector.BlackBoardKey);
    }

    public override void OnEnter()
    {
        frameIndex = 0;
        if (timeline && director)
        {
            director.SetTimeline(timeline);
            director.SetWarpMode(WarpMode);
            director.StartTimeline();
        }
        base.OnEnter();
    }

    public override void OnUpdate(float deltaTime)
    {
        director.TimelineUpdate();
        base.OnUpdate(deltaTime);
    }

    public override void OnLogicUpdate()
    {
        director.LogicUpdate(frameIndex);
        frameIndex++;
        base.OnLogicUpdate();
    }
        

    public override void OnExit()
    {
        director.InterruptTimeline();
        base.OnExit();
    }
}