using System.Collections.Generic;
using Animancer;
using UnityEngine;

[CreateAssetMenu(menuName = "Animation/Animator Layer Sets", fileName = "NewAnimatorLayerSets")]
public class AnimatorLayerSets : ScriptableObject
{
    public List<StringAsset> layers;
}