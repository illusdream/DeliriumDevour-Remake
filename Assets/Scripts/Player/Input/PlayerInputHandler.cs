using System;
using System.Collections.Generic;
using Animancer.Units;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using StatModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerInputHandler : MonoBehaviour,IHostedLogicUpdate
{
    private PlayerInput input;

    public InputActionTracker<Vector2> Move { get; private set; }

    public InputActionTracker<Vector2> Look { get; private set; }
    
    public InputActionTracker Shift { get; private set; }
    
    public InputActionTracker Jump { get; private set; }

    /// <summary>
    /// 可以存储的逻辑帧数目
    /// </summary>
    private int stackFrameCount;

    [Seconds]
    public float InputBufferTime;
    
    private InputFrameInfoList InputFrames;
    
    public BaseActorActionStatModel actorActionStatModel;
    
    public void Awake()
    {
        input = InputManager.Instance.GetCurrentInputAction();
        stackFrameCount = (int)(InputBufferTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        InputFrames = new InputFrameInfoList(stackFrameCount);
        InitAll();
        //将其封装到一个操作封装到一个List，用于支持快速查找
    }

    public void OnEnable()
    {
        InputManager.Instance.RegisterHostedUpdate(this);
    }

    public void Start()
    {

    }

    private void InitAll()
    {
        Move = new InputActionTracker<Vector2>(input.Player.Move);
        Move.performed += context => AddSingleFrameInfo(EPlayerInput.Move, context); 
        
        Look = new InputActionTracker<Vector2>(input.Player.Look);
       // Look.performed += context => AddSingleFrameInfo(EPlayerInput.Look, context);
        
        Shift = new InputActionTracker(input.Player.Shift);
        Shift.performed += context => AddSingleFrameInfo(EPlayerInput.Shift, context);
        
        Jump = new InputActionTracker(input.Player.Jump);
        Jump.performed += context => AddSingleFrameInfo(EPlayerInput.Jump, context);
        
    }

    private void AddSingleFrameInfo(EPlayerInput playerInput,InputAction.CallbackContext context)
    {
        var result = new SingleInputInfo()
        {
            FrameID = FrameworkCore.Instance.LogicFrameIndex,
            PlayerInput = playerInput,
            InputInteraction = context.interaction switch
            {
                HoldInteraction  => EInputInteraction.Hold,
                MultiTapInteraction  => EInputInteraction.MultiTap,
                PressInteraction  => EInputInteraction.Press,
                SlowTapInteraction  =>EInputInteraction.SlowTap,
                TapInteraction  =>EInputInteraction.Tap,
                null => EInputInteraction.None,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        InputFrames.Current.AddInputInfo(result);
    }
    
    public void HostedLogicUpdate()
    {
        InputFrameInfo currentFrameInfo = new InputFrameInfo();
        InputFrames.Add(currentFrameInfo);
    }

    public bool HasTriggered(EPlayerInput playerInput, EInputInteraction interaction, float duration)
    {
        //根据时间转换到具体的表中
        if (actorActionStatModel && actorActionStatModel.BlockInput.Contains(playerInput.ToString()))
        {
            return false;
        }
        return InputFrames.HasTriggered(playerInput, interaction, duration);
    }

    public void OnDisable()
    {
        InputManager.Instance?.UnregisterHostedUpdate(this);
    }
}