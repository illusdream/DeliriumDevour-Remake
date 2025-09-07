using System;
using System.Linq;
using Animancer;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace ilsActionEditor
{
    
    public class SingleAnimationClip : BaseActionClip
    {
        [LeftToggle]
        public bool JustPlay;
        
        [SerializeField]
        [DisableInInlineEditors]
        [HideInInspector]
        private ClipTransition _animation;
        
        [ShowInInspector]
        [PropertyOrder(-int.MaxValue)]
        [HideReferenceObjectPicker]
        public ClipTransition UseAnimation
        {
            get { return _animation; }
            set
            {
                _animation = value;
                _length = _animation.Length;
            }
        }

        public override string info
        {
            get
            {
                return  UseAnimation != null ? UseAnimation.Clip.name : base.info;
            }
        }

        private AnimancerState _state;

        public AnimancerState State
        {
            get { return _state; }
            private set=>_state = value;
        }
        [SerializeField]
        [HideInInspector]
        public float _length;

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

        public float clipOffset;
        
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
                _state = _animancer.Play(_animation,_animation.FadeDuration,FadeMode.FromStart);
                _state.Time = clipOffset;
            }
            else
            {
                _state = _animancer.Play(_animation,_animation.FadeDuration,FadeMode.FromStart);
                _state.IsPlaying = false;
                _state.Time = clipOffset;
                nowTime = clipOffset;;
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
            if ( UseAnimation.Clip ) {
                EditorTools.DrawLoopedLines(rect, UseAnimation.Clip.length / Speed, this.length, clipOffset);
            }
            base.OnClipGUI(rect);
        }
    }
}