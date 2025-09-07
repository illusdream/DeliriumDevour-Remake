using System;
using Animancer;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestAnimationController : MonoBehaviour
    {
        public Animator animator;

        public Vector3 deltaPosition;
        
        [SerializeReference]
        public TransitionAsset clipTransition;
        public AnimancerComponent animancerComponent;

        [Button]
        public void Play(int layer)
        {
            animancerComponent.Layers[layer].Play(clipTransition);
            if (layer > 0)
            {
                animancerComponent.Layers[layer].StartFade(0,clipTransition.MaximumDuration);
            }
        }
        public void Update()
        {
           // transform.position += deltaPosition;
        }

        public void OnAnimatorMove()
        {
           // transform.position +=  animator.deltaPosition;
        }
    }
}