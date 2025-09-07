using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionTracker : IDisposable 
{
    public InputAction _trackedAction { get; private set; }

    public float StartRealTime { get; private set; }
    public float StartScaledTime { get; private set; }

    public float EndRealTime { get; private set; }
    public float EndScaledTime { get; private set; }
    
    /// <summary>
    /// 最后触发的时间
    /// </summary>
    public float LastTriggerInvokeTime { get; private set; }
    

    public float ContinueRealTime => EndRealTime > StartRealTime ? EndRealTime - StartRealTime :0;
    public float ContinueScaledTime =>EndScaledTime > StartScaledTime ? EndScaledTime - StartScaledTime :0;

    public event Action<InputAction.CallbackContext> started;
    public event Action<InputAction.CallbackContext> performed;
    public event Action<InputAction.CallbackContext> canceled;
    public InputActionTracker(InputAction trackedAction)
    {
        _trackedAction = trackedAction;
        
        _trackedAction.started += TrackedActionOnstarted;
        _trackedAction.performed += TrackedActionOnperformed;
        _trackedAction.canceled += TrackedActionOncanceled;
    }

    private void TrackedActionOnstarted(InputAction.CallbackContext obj)
    {
        StartRealTime = Time.realtimeSinceStartup;
        StartScaledTime = Time.time;
        
        EndRealTime = Time.realtimeSinceStartup;
        EndScaledTime = Time.realtimeSinceStartup;
        
        LastTriggerInvokeTime = Time.realtimeSinceStartup;
        started?.Invoke(obj);
    }
    private void TrackedActionOnperformed(InputAction.CallbackContext obj)
    {
        EndRealTime = Time.realtimeSinceStartup;
        EndScaledTime = Time.realtimeSinceStartup;
        
        LastTriggerInvokeTime = Time.realtimeSinceStartup;
        performed?.Invoke(obj);
    }
    private void TrackedActionOncanceled(InputAction.CallbackContext obj)
    {
        EndRealTime = Time.realtimeSinceStartup;
        EndScaledTime = Time.realtimeSinceStartup;
        
        LastTriggerInvokeTime = Time.realtimeSinceStartup;
        canceled?.Invoke(obj);
    }
    
    public void Update()
    {
        EndRealTime = Time.realtimeSinceStartup;
        EndScaledTime = Time.realtimeSinceStartup;
    }
    /// <summary>
    /// 是否被触发
    /// </summary>
    /// <param name="duration">缓存时间</param>
    public bool HasTriggered(float duration)
    {
        float cTime = Time.realtimeSinceStartup;
        if (duration == 0)
        {
            return (cTime - LastTriggerInvokeTime) < Time.unscaledTime;
        }
        else
        {
            return (cTime - LastTriggerInvokeTime) <= duration;
        }
    }

    public void ResetTriggers()
    {
        LastTriggerInvokeTime = -1;
    }
    public void Dispose()
    {
        
    }

    public static implicit operator InputAction(InputActionTracker trackedAction)
    {
        return trackedAction._trackedAction;
    }

}

public class InputActionTracker<T> : InputActionTracker where T : struct
{
    public InputActionTracker(InputAction trackedAction) : base(trackedAction)
    {
    }

    public T ActionValue => (_trackedAction?.ReadValue<T>()).GetValueOrDefault();
}