using System;
using UnityEngine;

namespace ilsActionEditor
{
    [TargetClip(typeof(SingleAnimationClip),typeof(Mixer2DAnimationClip),typeof(LinearMultiMixerAnimationClip))]
    public class AnimationTrack : BaseActionTrack
    {
        public string AnimancerBlackBoardName;

        protected override bool OnInitialize()
        {
            return base.OnInitialize();
        }

        public override void OnTrackInfoGUI(Rect trackRect)
        {
            base.OnTrackInfoGUI(trackRect);
        }
    }
}