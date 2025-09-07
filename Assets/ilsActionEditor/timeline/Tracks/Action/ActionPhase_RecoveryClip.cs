using System.Collections.Generic;
using Animancer;
using ilsFramework.Core;
using StatModel;
using UnityEngine;

namespace ilsActionEditor
{
    public class ActionPhase_RecoveryClip : BaseActionClip
    {
        [SerializeField]
        [HideInInspector]
        public float _length= 0.1f;

        public override float length
        {
            get { return _length; }
            set { _length = value; }
        }
        public int startFrame =>Mathf.CeilToInt( startTime * ILogicDirectable.LogicFrameCountPerSecond);
        public int endFrame=>Mathf.CeilToInt( endTime * ILogicDirectable.LogicFrameCountPerSecond);
        
        public List<StringAsset> Cancels;
        
        public List<StringAsset> Blocks;
        
        private BaseActorActionStatModel _actorActionStatModel;

        public override bool OnInitialize()
        {
            _actorActionStatModel = BlackBoard.GetValue<BaseActorActionStatModel>(BaseActorActionStatModel.BlackBoardKey);
            return base.OnInitialize();
        }

        public override void OnEnter()
        {
            if (_actorActionStatModel)
            {
                foreach (var cancel in Cancels)
                {
                    _actorActionStatModel.RecoveryCancels.Add(cancel.ToString());
                }
                
                foreach (var block in Blocks)
                {
                    _actorActionStatModel.BlockInput.Add(block);
                }
            }
            base.OnEnter();
        }

        public override void OnLogicUpdate()
        {
            if (_actorActionStatModel)
            {
                foreach (var cancel in Cancels)
                {
                    _actorActionStatModel.RecoveryCancels.Add(cancel.ToString());
                }
                
                foreach (var block in Blocks)
                {
                    _actorActionStatModel.BlockInput.Add(block);
                }
            }
            base.OnLogicUpdate();
        }
        
        public override void OnExit()
        {
            if (_actorActionStatModel)
            {
                foreach (var cancel in Cancels)
                {
                    _actorActionStatModel.RecoveryCancels.Remove(cancel.ToString());
                }
                
                foreach (var block in Blocks)
                {
                    _actorActionStatModel.BlockInput.Remove(block);
                }
            }
            base.OnExit();
        }
#if UNITY_EDITOR
        protected override void OnClipLengthChanged()
        {
            //修改长度，使得可以一直衔接
            if (parent is ActionPhasesTrack ap && ap.ExecutionClip)
            {
                ap.ExecutionClip.endTime = startTime;
            } 
            endTime = parent.endTime;
            base.OnClipLengthChanged();
        }

        public override string info => "RecoveryPhase";
#endif
    }
}