using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Animancer.Units;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using Slate;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ilsActionEditor
{  
    //先只提供基础的timeline功能（能动就行）
    [CreateAssetMenu(fileName = "ActionTimeline", menuName = "ilsActionEditor/ActionTimeline")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class ActionTimeline : ScriptableObject, IDirectable
    {
        /// <summary>
        /// 每秒钟帧数，这个应该不改的是固定参数
        /// </summary>
        [LabelText("每秒帧数")]
        public int FramePreSecond = 50;
        
        [SerializeField]
        private List<BaseActionTrack> _tracks = new List<BaseActionTrack>();
        


        
        [SerializeField, HideInInspector]
        //用来标记这个timeline资源是否有效的，如果无效则无法播放
        private bool _active = true;
        //————————————————————————————————————
        //没有Lock和Collapsed ，因为timeline自身不该可以展开和锁定-》本身就是一定要展开和允许修改的
        private bool _isLocked => false;
        
        private bool _isCollapsed => false;
        //————————————————————————————————————

        //运行时状态，主要目的是给Running前后添加标识符
        private bool _isRunning = false;

        public bool IsRunning
        {
            get => _isRunning;
            set => _isRunning = value;
        }
        
        ///<summary>The child tracks</summary>
        public List<BaseActionTrack> tracks {
            get { return _tracks; }
            set { _tracks = value; }
        }
        
        public IEnumerable<IDirectable> children { get { return tracks.Cast<IDirectable>(); } }
        public GameObject actor { get; }
        public float startTime { get { return 0; } }

        public int startFrame { get { return 0; } }
        
        /// <summary>
        /// 实际的最终时间
        /// </summary>
        [SerializeField]
        private float _endTime;

        [ShowInInspector]
        public float endTime
        {
            get => _endTime;
            set => _endTime = value;
        }

        public int endFrame => Mathf.FloorToInt(endTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        public float blendIn { get { return 0f; } }
        public float blendOut { get { return 0f; } }
        public bool canCrossBlend { get { return false; } }
        public IDirectable parent { get { return null; } }
        public IDirector root { get; private set; }
        
        [ShowInInspector]
        public bool isActive {
            get { return _active; }
            set
            {
                if ( _active != value ) {
                    _active = value;
                    if ( root != null ) {
                        root.Validate();
                    }
                }
            }
        }

        public bool isCollapsed {
            get { return _isCollapsed; }
        }

        /// <summary>
        /// 永远没有Lock，因为这个已经是最顶级的了
        /// </summary>
        public bool isLocked {
            get { return _isLocked; }
        }
        
        //Validate the group and it's tracks
        public void Validate(IDirector root, IDirectable parent) {
            this.root = root;
           // var foundTracks = GetComponentsInChildren<CutsceneTrack>(true);
          //  for ( var i = 0; i < foundTracks.Length; i++ ) {
            //    if ( !tracks.Contains(foundTracks[i]) ) {
             //       tracks.Add(foundTracks[i]);
             //   }
           // }
           // if ( tracks.Any(t => t == null) ) { tracks = foundTracks.ToList(); }
            
        }
        
        public bool Initialize() {
            return true;
        }

        public void Enter()
        {
            
        }

        public void LogicEnter()
        {
            
        }

        public void Exit()
        {
            
        }

        public void LogicExit()
        {
            
        }

         void IDirectable.Update()
        {
           
        }

        public void LogicUpdate()
        {
            
        }

        public void ReverseEnter()
        {
          
        }

        public void Reverse()
        {
        
        }

        public void RootEnabled()
        {
            
        }

        public void RootUpdated(float time, float previousTime)
        {
            
        }

        public void RootDisabled()
        {
            
        }

        public void RootDestroyed()
        {
            
        }
        
        
        /*void IDirectable.LogicInitialize()
        {
            OnLogicInitialize();
        }
        public virtual void OnLogicInitialize() { }

        void IDirectable.LogicEnter()
        {
            OnLogicEnter();
        }
        public virtual void OnLogicEnter() { }

        void IDirectable.LogicUpdate(float time, float previousTime,int logicFrameCount)
        {
            OnLogicUpdate(time,previousTime,logicFrameCount);
        }
        public virtual void OnLogicUpdate(float time, float previousTime,int logicFrameCount) { }

        void IDirectable.LogicExit()
        {
            OnLogicExit();
        }
        public virtual void OnLogicExit() { }*/
#if UNITY_EDITOR
        public void DrawGizmos(bool selected)
        {
           
        }

        public void SceneGUI(bool selected)
        {
           
        }
#endif
        public List<IDirectable> GetAllDirectables()
        {
            var result = new List<IDirectable>();
            result.Add(this);
            result.AddRange(tracks);
            foreach (var track in tracks)
            {
                result.AddRange(track.clips);
            }
            return result;
        }
        
        ///<summary>Can track be added in this group?</summary>
        public bool CanAddTrack(BaseActionTrack track) {
            return track != null && CanAddTrackOfType(track.GetType());
        }

        ///<summary>Can track type be added in this group?</summary>
        public bool CanAddTrackOfType(System.Type type) {
            if ( type == null || !typeof(BaseActionTrack).IsAssignableFrom(type) || type.IsAbstract ) {
                return false;
            }
            return true;
        }
        ///----------------------------------------------------------------------------------------------
        /// TODO
        ///  还没做完
#if !UNITY_EDITOR 
        //runtime add/delete track
        ///<summary>Add a new track to this group</summary>
        public T AddTrack<T>(string name = null) where T : BaseActionTrack { return (T)AddTrack(typeof(T), name); }
        public BaseActionTrack AddTrack(System.Type type, string name = null) {

            return null;
        }

        ///<summary>Delete a track of this group</summary>
        public void DeleteTrack(BaseActionTrack track) {
            tracks.Remove(track);
            
            root.Validate();
        }
#endif
        ///----------------------------------------------------------------------------------------------

       ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        ///<summary>Add a new track to this group</summary>
        public T AddTrack<T>(string name = null) where T : BaseActionTrack { return (T)AddTrack(typeof(T), name); }
        [Button]
        public BaseActionTrack AddTrack(Type type, string name = null) {

            if (type == null)
            {
                type = typeof(BaseActionTrack);
            }
            if (!CanAddTrackOfType(type)) {
                return null;
            }

            var path = AssetDatabase.GetAssetPath(this);
            
            //创建一个新的
            var newTrack = ScriptableObject.CreateInstance(type) as BaseActionTrack;
            if ( name != null ) { newTrack.name = name; }

            AssetDatabase.AddObjectToAsset(newTrack,this );
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            tracks.Add(newTrack);
            
            newTrack.PostCreate(this);
            root?.Validate();
            return newTrack;
        }

        ///<summary>Delete a track of this group</summary>
        /// TODO:
        ///  保存在UNity中的部分（重做）
        public void DeleteTrack(BaseActionTrack track) {
            tracks.Remove(track);
            AssetDatabase.RemoveObjectFromAsset(track);
            Object.DestroyImmediate(track,true);
            AssetDatabase.SaveAssets();
        }

        ///<summary>Duplicate the track in this group</summary>
        public BaseActionTrack DuplicateTrack(BaseActionTrack track) {

            
            if ( !CanAddTrack(track) ) {
                return null;
            }
            var newTrack = (BaseActionTrack)Instantiate(track);
            AssetDatabase.AddObjectToAsset(newTrack,this );
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            tracks.Add(newTrack);
            AssetDatabase.Refresh();
            return newTrack;
        }

#endif
        
    }
}