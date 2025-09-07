using System;
using System.Collections;
using System.Collections.Generic;
using ilsFramework.Core;
using Sirenix.OdinInspector;

public class InputFrameInfoList : IEnumerable<InputFrameInfo>
{
    private int bufferCapacity;
    
    [ShowInInspector]
    private List<InputFrameInfo> frameInfos;
    
    public InputFrameInfo this[int index] => frameInfos[index];

    public int Count => frameInfos.Count;

    public InputFrameInfo Current
    {
        get
        {
            if (Count ==0)
            {
               Add(new InputFrameInfo());
            }
            return  frameInfos[0];
        }
    }
    
    public InputFrameInfoList(int bufferCapacity)
    {
        this.bufferCapacity = bufferCapacity;
        frameInfos = new List<InputFrameInfo>() { new InputFrameInfo() };
    }
    
    public void Add(InputFrameInfo frameInfo)
    {
        if (Count >= bufferCapacity)
        {
            frameInfos.RemoveAt(frameInfos.Count - 1);
        }
        frameInfos.Insert(0,frameInfo);
    }

    public bool HasTriggered(EPlayerInput playerInput, EInputInteraction interaction, float duration)
    {
        var maxRange = Math.Floor(duration * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);

        if (maxRange < 1)
        {
            maxRange = 1;
        }
        
        for (int i = 0; i < maxRange && i < frameInfos.Count; i++)
        {
            var frameInfo = frameInfos[i];
            if (frameInfo.TryGetInputInfo((playerInput, interaction), out var result))
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator<InputFrameInfo> GetEnumerator()
    {
        return frameInfos.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}