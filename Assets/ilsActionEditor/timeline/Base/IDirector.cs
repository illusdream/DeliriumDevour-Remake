using System.Collections.Generic;
using UnityEngine;

namespace ilsActionEditor
{

    ///<summary>IDirectable播放器的接口。这用于IDirectables与其root属性。</summary>
    public interface IDirector
    {
        IEnumerable<IDirectable> children { get; }
        GameObject context { get; }
        float length { get; }
        float currentTime { get; set; }
        float previousTime { get; }
        float playbackSpeed { get; set; }
        bool isActive { get; }
        bool isPaused { get; }
        bool isReSampleFrame { get; }
        IEnumerable<GameObject> GetAffectedActors();
        void Play();
        void Pause();
        void Stop();
        void ReSample();
        void Validate();
        void SendGlobalMessage(string message, object value);
    }
}