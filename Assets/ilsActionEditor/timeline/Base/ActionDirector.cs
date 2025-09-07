using System;
using System.Collections.Generic;
using System.Linq;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    //就是个执行器
    public class ActionDirector : MonoBehaviour,IDirector
    {
        public static string BlackBoardKey = "ActionDirector";
        public enum ActionDirectStage
        {
            Start,
            Running,
            End
        }

        private ActionDirectStage _stage;

        public ActionDirectStage Stage
        {
            get => _stage;
            private set => _stage = value;
        }
        
        private List<TimelineTimePoint> timelineTimePoints;
        
        [ShowInInspector]
        private ActionTimeline timeline;
        
        
        private List<IDirectable> directables;
        private bool timelineIsInitialized;
        
        public IEnumerable<IDirectable> children { get; }
        public GameObject context { get; }
        public float length =>(timeline?.GetLength()).GetValueOrDefault();

        private float _currentTime;
        public float currentTime {
            get { return _currentTime; }
            set { _currentTime = Mathf.Clamp(value, 0, length); }
        }
        public float previousTime { get;private set; }
        
        public int currentFrameIndex { get; private set; }
        public int previousFrameIndex { get; private set; }
        public float playbackSpeed { get; set; }
        public bool isActive { get; }
        public bool isPaused { get; }
        private bool _isReSampleFrame;
        public bool isReSampleFrame { get; }
        
        private TimelineWarpMode timelineWarpMode;
        
        public TimelineWarpMode TimelineWarpMode => timelineWarpMode;

        [SerializeField,HideInInspector]
        private MonoBlackBoard blackBoard;
        [ShowInInspector]
        public MonoBlackBoard BlackBoard
        {
            get => blackBoard;
            set => blackBoard = value;
        }
        
        public Action OnActionComplete;
        
        protected void Awake()
        {
        }
        
        public IEnumerable<GameObject> GetAffectedActors()
        {
            return null;
        }

        public void SetTimeline(ActionTimeline timeline)
        {
            this.timeline = timeline;
            
            timelineIsInitialized = false;
            Validate(); 
        }
        
        public void Play() { Play(0); }
        public void Play(System.Action callback) { Play(0, callback); }
        public void Play(float startTime) { Play(startTime, length); }
        public void Play(float startTime, System.Action callback) { Play(startTime, length, callback); }

        public void Play(float startTime, float endTime, System.Action callback = null)
        {

            /*if ( startTime > endTime && playDirection != PlayingDirection.Backwards ) {
                Debug.LogError("End Time must be greater than Start Time.", gameObject);
                return;
            }

            if ( isPaused ) { //if it's paused resume.
                Debug.LogWarning("Play called on a Paused cutscene. Cutscene will now resume instead.", gameObject);
                playingDirection = playDirection;
                Resume();
                return;
            }

            if ( isActive ) {
                Debug.LogWarning("Cutscene is already Running.", gameObject);
                return;
            }

            playTimeMin = 0; //for mathf.clamp setter

            playTimeMax = endTime;
            playTimeMin = startTime;
            currentTime = startTime;
            playingWrapMode = wrapMode;
            playingDirection = playDirection;

            if ( playDirection == PlayingDirection.Forwards ) {
                if ( currentTime >= playTimeMax ) {
                    currentTime = playTimeMin;
                }
            }

            if ( playDirection == PlayingDirection.Backwards ) {
                if ( currentTime <= playTimeMin ) {
                    currentTime = playTimeMax;
                }
            }

            isActive = true;
            isPaused = false;
            OnStop = callback != null ? callback : OnStop;

            Sample(); //immediately update once now instead of waiting LaterUpdate, FixedUpdate etc.
            // UpdateCutscene(Time.deltaTime);

            SendGlobalMessage("OnCutsceneStarted", this);
            if ( OnCutsceneStarted != null ) {
                OnCutsceneStarted(this);
            }*/
        }

        public void Pause()
        {
            
        }

        public void Stop()
        {
            
        }

        public void SetWarpMode(TimelineWarpMode warpMode)
        {
            timelineWarpMode = warpMode;
        }
        
        public void StartTimeline()
        {
            Stage = ActionDirectStage.Start;
            InitializeTimePointers();
            OnTimelineStarted();
            Stage = ActionDirectStage.Running;
        }
        
        public void TimelineUpdate()
        {
            foreach (var timePointer in timelineTimePoints)
            {
                timePointer.Update();
            }
        }
        public void LogicUpdate(int frameIndex)
        {
            currentFrameIndex = frameIndex;
            switch (TimelineWarpMode)
            {
                case TimelineWarpMode.Once:
                    if (currentFrameIndex < 0 || currentFrameIndex > timeline.endFrame)
                    {
                        return;
                    }
            
                    foreach (var timePointer in timelineTimePoints)
                    {
                        timePointer.LogicUpdate(currentFrameIndex);
                    }

                    if (currentFrameIndex == timeline.endFrame)
                    {
                        Stage = ActionDirectStage.End;
                    }
                    break;
                case TimelineWarpMode.Loop:
                    if (currentFrameIndex <0)
                    {
                        return;
                    }

                    if (currentFrameIndex > timeline.endFrame)
                    {
                        currentFrameIndex %= timeline.endFrame;
                    }
                    foreach (var timePointer in timelineTimePoints)
                    {
                        timePointer.LogicUpdate(currentFrameIndex);
                    }
                    if (currentFrameIndex == 0 && previousFrameIndex == timeline.endFrame -1)
                    {
                        Stage = ActionDirectStage.End;
                        //重新再生成一遍
                        StartTimeline();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            previousFrameIndex = currentFrameIndex;
        }
        
        public void EndTimeline()
        {
            if (Stage == ActionDirectStage.End)
            {
                return;
            }
            Stage = ActionDirectStage.End;
            OnTimelineEnded();
        }
        
        public void ReSample()
        {
            
        }

        public void Validate()
        {
            if (!timeline)
            {
                return;
            }
            directables = new List<IDirectable>();
            this.timeline.Validate(this,null);
            foreach ( IDirectable track in timeline.children.Reverse() ) {
                directables.Add(track);
                try { track.Validate(this, timeline); }
                catch ( System.Exception e ) { Debug.LogException(e); }
                foreach ( IDirectable clip in track.children ) {
                    directables.Add(clip);

                    try { clip.Validate(this, track); }
                    catch ( System.Exception e ) { Debug.LogException(e); }
                }
            }
        }

        public void SendGlobalMessage(string message, object value)
        {
            
        }
          void InitializeTimePointers()
          {
              timelineTimePoints = new List<TimelineTimePoint>();

              foreach (IDirectable track in timeline.children.Reverse())
              {
                  if (track.isActive && track.Initialize())
                  {
                      var pTrack = new TimelineTimePoint(track);
                      timelineTimePoints.Add(pTrack);
                      foreach (IDirectable clip in track.children)
                      {
                          if (clip.isActive && clip.Initialize())
                          {
                              var pClip = new TimelineTimePoint(clip);
                              timelineTimePoints.Add(pClip);
                          }
                      }
                  }
              }
              timelineIsInitialized = true;
          }

        //When Sample begins
        void OnTimelineStarted() {
            for ( var i = 0; i < timeline.tracks.Count; i++ ) {
                try { (timeline.tracks[i] as IDirectable)?.RootEnabled(); }
                catch ( System.Exception e ) { Debug.LogException(e, gameObject); }
                for (int j = 0; j < timeline.tracks[i].clips.Count; j++)
                {
                    try { (timeline.tracks[i].clips[j] as IDirectable)?.RootEnabled(); }
                    catch ( System.Exception e ) { Debug.LogException(e, gameObject); }
                }
            }
#if UNITY_EDITOR
            transform.hideFlags = HideFlags.NotEditable;
#endif
        }

        //When Sample ends
        void OnTimelineEnded() {
            for ( var i = 0; i < timeline.tracks.Count; i++ ) {
                try { (timeline.tracks[i] as IDirectable)?.RootDisabled(); }
                catch ( System.Exception e ) { Debug.LogException(e, gameObject); }
                for (int j = 0; j < timeline.tracks[i].clips.Count; j++)
                {
                    try { (timeline.tracks[i].clips[j] as IDirectable)?.RootDisabled(); }
                    catch ( System.Exception e ) { Debug.LogException(e, gameObject); }
                }
            }
#if UNITY_EDITOR
            transform.hideFlags = HideFlags.None;
#endif
        }

        public void InterruptTimeline()
        {
            if (Stage == ActionDirectStage.Running)
            {
                foreach (var timePointer in timelineTimePoints)
                {
                    timePointer.OnInterruptTimeline();
                }
                OnTimelineEnded();
                Stage = ActionDirectStage.End;
            }
        }
        public void OnValidate()
        {
            BlackBoard?.SetBlackBoardBinding(new BlackBoardMonoBinding(){key = BlackBoardKey, value = this});
        }
    }
}