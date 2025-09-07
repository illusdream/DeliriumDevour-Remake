using System;
using System.Collections.Generic;
using Animancer;
using Animancer.Editor;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 托管Animator和Animancer
/// </summary>
[RequireComponent(typeof(Animator), typeof(AnimancerComponent))]
public class AnimationHandler : MonoBehaviour,IHostedLogicUpdate
{
    public static string BlackBoardKey = "AnimationHandler";
    
    public AnimancerComponent animancerComponent;
    public Animator animator;
    public bool ApplyRootMotion = true;

    public Action<Animator,Vector3, Quaternion> RootMotionApply;

    public HashSet<AnimancerState> states = new HashSet<AnimancerState>();
    
    public float smoothTime = 0.5f;
    
    public float MaxSpeed = 1.0f;
    
    public AnimatorLayerSets Layers;

    public Dictionary<string, int> LayerMapping;
     
    //内置一个动画状态机，用于更新
    [SerializeField]
    [FoldoutGroup("动画状态机")]
    public AFSMTranslationMode AFSMTranslationMode;
    
    [FoldoutGroup("动画状态机")]
    public AFSMGraph FSMGraph;
        
    [ShowInInspector]
    [FoldoutGroup("动画状态机")]
    private AFSMGraph currentRunTimeGraph;
    [FoldoutGroup("动画状态机")]
    public MonoBlackBoard blackBoard;
    [FoldoutGroup("动画状态机")]
    public ActorLocomotionSets actorLocomotionSets;
    public void Awake()
    {
        
    }

    public virtual void Start()
    {
        currentRunTimeGraph = FSMGraph?.Copy() as AFSMGraph;
        if (currentRunTimeGraph)
        {
            currentRunTimeGraph.Initialize(blackBoard.blackBoard);
            currentRunTimeGraph.StartFSM();
            currentRunTimeGraph.AFSMTranslationMode = AFSMTranslationMode;
        }
        
        CheckLayerMapping();
    }

    public virtual  void OnEnable()
    {
        InputManager.Instance?.RegisterHostedUpdate(this);
        
    }

    public virtual  void Update()
    {
        currentRunTimeGraph?.DoUpdate(Time.deltaTime);
    }
    
    public virtual  void HostedLogicUpdate()
    {
        currentRunTimeGraph?.LogicUpdate();
    }

    public virtual  void FixedUpdate()
    {
        currentRunTimeGraph?.FixedUpdate();
    }

    public virtual  void LateUpdate()
    {
        currentRunTimeGraph?.LateUpdate();
    }

    public virtual  void OnDisable()
    {
        InputManager.Instance?.RegisterHostedUpdate(this);
    }

    public  virtual  AnimancerState Play(ITransition transition,string layerName = null)
    {
        CheckLayerMapping();
        var layerIndex = 0;
        if (layerName == null || !LayerMapping.TryGetValue(layerName, out layerIndex))
        {
            layerIndex = 0;
        }
        return animancerComponent.Layers[layerIndex].Play(transition);
    }


    public virtual  void OnAnimatorMove()
    {
        if (!ApplyRootMotion)
        {
            return;
        }
        RootMotionApply?.Invoke(animator,animator.deltaPosition, animator.deltaRotation);
    }

    public void OnValidate()
    {
        Transform root = transform;
        while (root.parent)
        {
            root = root.parent;
        }

        if (root.TryGetComponent<MonoBlackBoard>(out var blackBoard))
        {
            blackBoard.SetBlackBoardBinding(new BlackBoardMonoBinding(){key = BlackBoardKey, value = this});
        }
    }

    private void CheckLayerMapping()
    {
        if (LayerMapping == null)
        {
            LayerMapping = new Dictionary<string, int>();
            for (int i = 0; i < Layers.layers.Count; i++)
            {
                animancerComponent.Layers[i].SetDebugName(Layers.layers[i].name);
                LayerMapping[Layers.layers[i].name] = i;
            }
        }
    }
    
}