using Animancer;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    public class LinearMultiMixerAnimationClip : BaseActionClip
    {
        [LeftToggle]
        public bool JustPlay;
        
        [SerializeField]
        [DisableInInlineEditors]
        [HideInInspector]
        private MixerTransition2D _animation;
        
        
        [SerializeField]
        private MixerTransition2D _animation2;
        [SerializeField]
        private StringAsset linearMixKey;
        
        [ShowInInspector]
        [PropertyOrder(-int.MaxValue)]
        [HideReferenceObjectPicker]
        public MixerTransition2D UseAnimation
        {
            get { return _animation; }
            set
            {
                _animation = value;
            }
        }

        public override string info
        {
            get=> UseAnimation != null ? $"Mixer" : base.info;
        }

        private AnimancerState _state;

        public AnimancerState State
        {
            get { return _state; }
            private set=>_state = value;
        }
        [SerializeField]
        [HideInInspector]
        public float _length =1;

        public override float length
        {
            get { return _length; }
            set { _length = value; }
        }

        public override float blendIn
        {
            get
            {
                return (UseAnimation?.FadeDuration).GetValueOrDefault(0);
            }
            set
            {
                if (UseAnimation != null)
                {
                    UseAnimation.FadeDuration = value;
                }
            }
        }
        [ShowInInspector]
        [PropertyRange(0,2)]
        public float Speed
        {
            get
            {
                if (UseAnimation == null)
                {
                    return 1;
                }
                return UseAnimation.Speed;
            }
            set
            {
                if (UseAnimation != null)
                {
                    UseAnimation.Speed = value;
                }
            }
        }
        
        private AnimationStartPlayMode _startPlayMode;
        
        AnimancerComponent _animancer;
        AnimationHandler handler;


        
        public override bool OnInitialize()
        {
            handler = BlackBoard.GetValue<AnimationHandler>(AnimationHandler.BlackBoardKey);
            _animancer = handler.animancerComponent;
            return base.OnInitialize();
        }

        public override void OnEnter()
        {
            if (JustPlay)
            {
                if (_state == null)
                {
                    var parentMixer = new LinearMixerState();
                    _state = _animancer.Play(parentMixer,0.25f);
                    parentMixer.Add(_animation,0);
                    parentMixer.Add(_animation2,1);
                    parentMixer.ParameterName = linearMixKey;
                }
                else
                {
                    _animancer.Play(_state);
                }
            }
            else
            {
                _state = _animancer.Play(_animation);
                _state.IsPlaying = false;
            }

            base.OnEnter();
        }

        private float nowTime = 0;
        public override void OnUpdate()
        {            
            if (JustPlay)
            {
                
            }
        }

        public override void OnExit()
        {
            
        }
        protected override void OnClipGUI(Rect rect)
        {
            base.OnClipGUI(rect);
        }
    }
}