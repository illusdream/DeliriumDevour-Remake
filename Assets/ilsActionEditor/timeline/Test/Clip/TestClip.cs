using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor.Test.Clip
{
    public class TestClip : BaseActionClip,ILogicDirectable
    {
        [SerializeField]
        private float _length =1;

        public Color TestColor;
        
        public override float length
        {
            get => _length;
            set => _length = value;
        }

        public override void OnEnter()
        {
            Debug.Log($"逻辑帧进入");
            base.OnEnter();
        }

        public override void OnExit()
        {
            Debug.Log($"逻辑帧退出");
            base.OnExit();
        }

        public override void OnUpdate()
        {
  
            base.OnUpdate();
        }

        public override void OnRootUpdated(float time, float previousTime)
        {
            //Debug.Log($"Root:  time:{time}, previousTime:{previousTime}");
            base.OnRootUpdated(time, previousTime);
        }

        protected override void OnClipGUI(Rect rect)
        {
            GUI.color = TestColor;
            
            GUI.Box(rect,"",Styles.clipBoxHorizontalStyle);
            GUI.color = Color.white;
            base.OnClipGUI(rect);
        }
        [ShowInInspector]
        public int startFrame => Mathf.CeilToInt(startTime / 0.02f);
        [ShowInInspector]
        public int endFrame => Mathf.CeilToInt(endTime / 0.02f);
        public void LogicInitialize()
        {
            
        }

        public override void OnLogicUpdate()
        {
            Debug.Log($"逻辑帧更新");
            base.OnLogicUpdate();
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