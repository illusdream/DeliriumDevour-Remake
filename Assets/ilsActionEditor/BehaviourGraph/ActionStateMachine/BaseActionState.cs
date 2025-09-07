using ilsFramework.Core;
using UnityEngine;

namespace ilsActionEditor
{
    public class BaseActionState : AFSMStateNode
    {
        [ShowInGraph]
        public ActionTimeline timeline;
        [HideInInspector]
        public ActionDirector director;
        
        [ShowInGraph]
        public TimelineWarpMode WarpMode;
        

        private int frameIndex;
        public override void OnInit()
        {
            director = BlackBoard.GetValue<ActionDirector>(ActionDirector.BlackBoardKey);
            base.OnInit();
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
}