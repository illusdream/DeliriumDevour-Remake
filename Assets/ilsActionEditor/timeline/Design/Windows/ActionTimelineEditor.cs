#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Animancer.Editor;
using ilsFramework.Core;
using Object = UnityEngine.Object;

namespace ilsActionEditor
{
    /// <summary>
    /// 好jb长的编辑器，我服了
    /// </summary>
    public class ActionTimelineEditor : EditorWindow
    {
        //编辑器播放模式
        enum EditorPlaybackState
        {
            //停止播放
            Stoped,
            //向前播放
            PlayingForwards,
        }
        
        //循环模式
        enum EditorWarpMode
        {
            Once,
            Loop
        }

        /// <summary>
        /// 应该是导轨的颜色
        /// </summary>
        struct GuideLine
        {
            public float time;
            public Color color;
            public GuideLine(float time, Color color) {
                this.time = time;
                this.color = color;
            }
        }

        ///----------------------------------------------------------------------------------------------


        public static ActionTimelineEditor current;

        public static event Action OnStopInEditor;

        /// <summary>
        /// 正在编辑的实例
        /// </summary>
        private ActionTimeline editorInstance;
        /// <summary>
        /// 正在编辑的实例的GUID
        /// </summary>
        private string editorInstanceGUID;

        public ActionTimeline actionTimeline {
            get
            {
               return editorInstance;
            }
            private set
            {
                editorInstance = value;
            }
        }

        private GameObject actingPrefab;

        private ActionDirector _currentActionDirector;

        private ActionDirector currentActionDirector
        {
            get
            {
                if (!_currentActionDirector)
                {
                    _currentActionDirector = defaultEditorActionDirector.GetComponent<ActionDirector>();
                }
                return _currentActionDirector;
            }
        }
        
        private GameObject _defaultEditorActionDirector;

        private GameObject defaultEditorActionDirector
        {
            get
            {
                if (!_defaultEditorActionDirector)
                {
                    _defaultEditorActionDirector = new GameObject("Default Editor Action Director");
                    _defaultEditorActionDirector.AddComponent<ActionDirector>();
                }
                
                return _defaultEditorActionDirector;
            }
        }
            
        #region UI布局相关的变量

        /// <summary>
        /// 为了保持一致性。左边的空白处。Group或者Track列表的宽度。
        /// </summary>
        private static float LEFT_MARGIN { //caps for consistency. margin on the left side. The width of the group/tracks list.
            get { return Prefs.trackListLeftMargin; }
            set { Prefs.trackListLeftMargin = Mathf.Clamp(value, 230, 400); }
        }
        private const float RIGHT_MARGIN = 16; //右侧边距
        private const float TOOLBAR_HEIGHT = 21; //工具栏的高度
        private const float TOP_MARGIN = 40; //工具栏后的上边距
        private const float GROUP_HEIGHT = 22; //组标题高度
        private const float TRACK_MARGINS = 4;  //同一组track之间的边距（顶部/底部）
        private const float GROUP_RIGHT_MARGIN = 4;  //组右侧的边距
        private const float TRACK_RIGHT_MARGIN = 4;  //轨道右侧的边缘
        private const float FIRST_GROUP_TOP_MARGIN = 22; //初始上边距prio
        private const float ADDTRACK_GROUP_HEIGHT = 200;//添加轨道的大小

        private static readonly Color LIST_SELECTION_COLOR = new Color(0.5f, 0.5f, 1, 0.3f);
        private static readonly Color GROUP_COLOR = new Color(0f, 0f, 0f, 0.25f);
        private Color HIGHLIGHT_COLOR { get { return isProSkin ? new Color(0.65f, 0.65f, 1) : new Color(0.1f, 0.1f, 0.1f); } }
        private float MAGNET_SNAP_INTERVAL { get { return viewTime * 0.01f; } }

        //Layout Rects
        private Rect topLeftRect;   //for playback controls
        private Rect topMiddleRect; //for time info
        private Rect leftRect;      //for group/track list
        private Rect centerRect;    //for timeline

        #endregion

        [System.NonSerialized] private Dictionary<int, ActionClipWrapper> clipWrappers;
        [System.NonSerialized] private Dictionary<BaseActionClip, ActionClipWrapper> clipWrappersMap;
        [System.NonSerialized] private EditorPlaybackState editorPlaybackState = EditorPlaybackState.Stoped;
        [System.NonSerialized] private EditorWarpMode editorWarpMode = EditorWarpMode.Once;
        [System.NonSerialized] private Rect LengthTragRect;
        [System.NonSerialized] private ActionClipWrapper interactingClip;
        [System.NonSerialized] private bool isMovingScrubCarret;
        [System.NonSerialized] private bool isMovingEndCarret;
        [System.NonSerialized] private bool isMouseButton2Down;
        [System.NonSerialized] private Vector2 scrollPos;
        [System.NonSerialized] private float totalHeight;
        [System.NonSerialized] private BaseActionTrack pickedTrack;
        [System.NonSerialized] private float lastStartPlayTime;
        [System.NonSerialized] private float editorPreviousTime;

        [System.NonSerialized] private Vector2? multiSelectStartPos;
     //   [System.NonSerialized] private List<ActionClipWrapper> multiSelection;
        [System.NonSerialized] private Rect preMultiSelectionRetimeMinMax;
        [System.NonSerialized] private int multiSelectionScaleDirection;

        [System.NonSerialized] private Vector2 mousePosition;
        [System.NonSerialized] private bool willRepaint;
        [System.NonSerialized] private bool willDirty;
        [System.NonSerialized] private bool willResample;
        [System.NonSerialized] private System.Action onDoPopup;
        [System.NonSerialized] private bool isResizingLeftMargin;
        [System.NonSerialized] private bool isAboutButtonPressed;
        [System.NonSerialized] private bool showDragDropInfo;
        [System.NonSerialized] private string searchString;
        [System.NonSerialized] private string addTrackSearchString = "";
        [System.NonSerialized] private float[] magnetSnapTimesCache;
        [System.NonSerialized] private List<GuideLine> pendingGuides;
        [System.NonSerialized] private System.Action postWindowsGUI;

        [System.NonSerialized] private BaseActionTrack copyTrack;

        

        [System.NonSerialized] private float timeInfoStart;
        [System.NonSerialized] private float timeInfoEnd;
        [System.NonSerialized] private float timeInfoInterval;
        [System.NonSerialized] private float timeInfoHighMod;

        [System.NonSerialized] private string webMessage;
        
        ///----------------------------------------------------------------------------------------------
        
        private static bool isProSkin {
            get { return EditorGUIUtility.isProSkin; }
        }

        /// <summary>
        /// 目前编辑器内的时间
        /// </summary>
        private float currentTime;
        
        /// <summary>
        /// 前一帧时间
        /// </summary>
        private float previousTime;

        private float logicUpdateTimer;
        public float length {
            get { return (actionTimeline?.endTime).GetValueOrDefault(1); }
            set { actionTimeline.endTime = value; }
        }
        private float _viewTimeMin;

        public float viewTimeMin
        {
            get { return _viewTimeMin; }
            set
            {
                _viewTimeMin = value;
            }
        }

        //The max view time
        private float _viewTimeMax;
        public float viewTimeMax {
            get { return _viewTimeMax; }
            set
            {
                _viewTimeMax = value;
            }
        }

        //The max time currently in view
        public float maxTime {
            get { return Mathf.Max(viewTimeMax, length); }
        }

        //The "length" of the currently viewing time
        public float viewTime {
            get { return viewTimeMax - viewTimeMin; }
        }
        private static Texture2D whiteTexture {
            get { return Styles.whiteTexture; }
        }
        public static void ShowWindow() { ShowWindow(null); }
        public static void ShowWindow(ActionTimeline editorInstance) {
            var window = EditorWindow.GetWindow(typeof(ActionTimelineEditor)) as ActionTimelineEditor;
            window.InitializeAll(editorInstance);
            window.Show();
        }
        [MenuItem("ilsFramework/动作timeline编辑器")]
        private static void ToolBarShowWindow()
        {
            //
            if (Selection.activeObject is ActionTimeline target)
            {
                ShowWindow(target);
                return;
            }
            ShowWindow();
        }

        private int FrameRate => 50;

        private bool willRun;
        
        //Screen Width. Handles retina.
        private static float screenWidth {
            get { return Screen.width / EditorGUIUtility.pixelsPerPoint; }
        }

        //Screen Height. Hanldes retina.
        private static float screenHeight {
            get { return Screen.height / EditorGUIUtility.pixelsPerPoint; }
        }
        private Color scruberColor {
            get { return (actionTimeline?.isActive).GetValueOrDefault(false) ? Color.yellow : new Color(1, 0.3f, 0.3f); }
        }
        float TimeToPos(float time) {
            return ( time - viewTimeMin ) / viewTime * centerRect.width;
        }

        //Convert position to time
        float PosToTime(float pos) {
            return ( pos - LEFT_MARGIN ) / centerRect.width * viewTime + viewTimeMin;
        }
        
        //Round time to nearest working snap interval
        float SnapTime(float time) {
            //holding control for precision (ignore snap intervals)
            if ( Event.current.control ) { return time; }
            return ( Mathf.Round(time / Prefs.snapInterval) * Prefs.snapInterval );
        }

        // TODO
        //Do action safely (stop cutscene, do, resample)
        void SafeDoAction(System.Action call) {
            var time = currentTime;
            //Stop(true);
            call();
            currentTime = time;
        }
        
        //Is directable filtered out by search string?
        bool IsFilteredOutBySearch(IDirectable directable, string search) {
            if ( string.IsNullOrEmpty(search) ) { return false; }
            if ( string.IsNullOrEmpty(directable.name) ) { return true; }
            return !directable.name.ToLower().Contains(search.ToLower());
        }
        
        //Draw a vertical guide line at time with color
        void DrawGuideLine(float time, Color color) {
            if ( time >= viewTimeMin && time <= viewTimeMax ) {
                var xPos = TimeToPos(time);
                var guideRect = new Rect(xPos + centerRect.x - 1, centerRect.y, 2, centerRect.height);
                GUI.color = color;
                GUI.DrawTexture(guideRect, whiteTexture);
                GUI.color = Color.white;
            }
        }
        
        //Add a cursor type at rect
        void AddCursorRect(Rect rect, MouseCursor type) {
            EditorGUIUtility.AddCursorRect(rect, type);
            willRepaint = true;
        }
        
        //Pop action GUI calls in popup
        void DoPopup(System.Action call) {
            onDoPopup = call;
        }
        
        //Cache an array of snap times for clip (clip times are excluded)
        //Saved in property .magnetSnapTimesCache
        void CacheMagnetSnapTimes(BaseActionClip clip = null) {
            var result = new List<float>();
            result.Add(0);
            result.Add(length);
            result.Add(currentTime);
            foreach ( var cw in clipWrappers ) {
                var action = cw.Value.clip;
                //exclude the target clip and only include the same group
                if ( clip == null || ( action != clip && action.parent.parent == clip.parent.parent ) ) {
                    result.Add(action.startTime);
                    result.Add(action.endTime);
                }
            }
            magnetSnapTimesCache = result.Distinct().ToArray();
        }
        
        //Find best snap time (closest)
        float? MagnetSnapTime(float time, float[] snapTimes) {
            if ( snapTimes == null ) { return null; }
            var bestDistance = float.PositiveInfinity;
            var bestTime = float.PositiveInfinity;
            for ( var i = 0; i < snapTimes.Length; i++ ) {
                var snapTime = snapTimes[i];
                var distance = Mathf.Abs(snapTime - time);
                if ( distance < bestDistance ) {
                    bestDistance = distance;
                    bestTime = snapTime;
                }
            }
            if ( Mathf.Abs(bestTime - time) <= MAGNET_SNAP_INTERVAL ) {
                return bestTime;
            }
            return null;
        }
        //TODO
        void OnEnable() {
            Styles.Load();
            Prefs.frameRate = 50;
#if UNITY_2018_3_OR_NEWER
            //UnityEditor.SceneManagement.PrefabStage.prefabStageClosing += (stage) => { if ( cutscene != null && stage.IsPartOfPrefabContents(cutscene.gameObject) ) { Stop(true); } };
#endif
           // UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnWillSaveScene;
            //UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnWillSaveScene;

#pragma warning disable 618
            EditorApplication.playmodeStateChanged -= InitializeAll;
            EditorApplication.playmodeStateChanged += InitializeAll;
#pragma warning restore

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
#if UNITY_2019_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

            Tools.hidden = false;
            titleContent = new GUIContent("ActionTimeline", Styles.cutsceneIconOpen);
            wantsMouseMove = true;
            autoRepaintOnSceneChange = false;
            minSize = new Vector2(500, 500);
            willRepaint = true;
            showDragDropInfo = true;
            pendingGuides = new List<GuideLine>();

            current = this;
            InitializeAll();
        }
        //TODO
        void OnDisable() {
          //  UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnWillSaveScene;

#pragma warning disable 618
            EditorApplication.playmodeStateChanged -= InitializeAll;
#pragma warning restore

            EditorApplication.update -= OnEditorUpdate;
#if UNITY_2019_3_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            Tools.hidden = false;
            //if ( cutscene != null && !Application.isPlaying ) {
              //  Stop(true);
           // }
            current = null;
        }
        
        //TODO
        //Set a new view when a script is selected in Unity's tab
        void OnSelectionChange() {
            if (Selection.activeObject != null ) {
                if (Selection.activeObject is ActionTimeline atl)
                {
                    if (atl != null && atl != actionTimeline)
                    {
                        InitializeAll(atl);
                    }
                }
            }
        }
        //Before scene is saved we need to stop so that cutscene changes are reverted.
        void OnWillSaveScene(UnityEngine.SceneManagement.Scene scene, string path) {
            //if ( cutscene != null && cutscene.currentTime > 0 ) {
               // Stop(true);
               // Debug.LogWarning("Scene Saved while a cutscene was in preview mode. Cutscene was reverted before saving the scene along with changes it affected.");
           // }
        }

        ///<summary>Initialize everything</summary>
        void InitializeAll() { InitializeAll(actionTimeline); }
        void InitializeAll(ActionTimeline newActionTimeline) {

            //first stop current cut if any
            if ( actionTimeline != null ) {
                if ( !Application.isPlaying ) {
                    Stop(true);
                }
            }

            //set the new
            if ( newActionTimeline != null ) {
                actionTimeline = newActionTimeline;
                UpdateViewRange();
                
               // multiSelection = null;
                InitClipWrappers();
                if ( !Application.isPlaying ) {
                    Stop(true);
                }
            }

            willRepaint = true;
        }

        void UpdateViewRange()
        {
            viewTimeMin = actionTimeline.startTime;
            viewTimeMax = actionTimeline.endTime;
        }

        void ShowLinkCursor(Rect? position = null)
        {
            position ??= GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(position.Value, MouseCursor.Link);
        }
        
        //initialize the action clip wrappers
        void InitClipWrappers() {

            if ( actionTimeline == null ) {
                return;
            }

           // multiSelection = null;
            var lastTime = currentTime;

            if ( !Application.isPlaying ) {
                Stop(true);
            }

           // actionTimeline.Validate();
            clipWrappers = new Dictionary<int, ActionClipWrapper>();
            clipWrappersMap = new Dictionary<BaseActionClip, ActionClipWrapper>();
            for ( int t = 0; t < actionTimeline.tracks.Count; t++ ) {
                for ( int a = 0; a < actionTimeline.tracks[t].clips.Count; a++ ) {
                    var id = UID(t, a);
                    if ( clipWrappers.ContainsKey(id) ) {
                        Debug.LogError("Collided UIDs. This should really not happen but it did!");
                        continue;
                    }
                    var clip = actionTimeline.tracks[t].clips[a];
                    var wrapper = new ActionClipWrapper(clip);
                    clipWrappers[id] = wrapper;
                    clipWrappersMap[clip] = wrapper;
                }
            }

            if ( lastTime > 0 ) {
                currentTime = lastTime;
            }
        }

        //An integer UID out of list indeces (track, action clip)
        int UID(int t, int a) {
            var B = t.ToString("D3");
            var C = a.ToString("D4");
            return int.Parse(B + C);
        }
        
        //Play button pressed or otherwise started
        public void Play(System.Action callback = null) {
            try
            {
                currentActionDirector.SetTimeline(actionTimeline);
                currentActionDirector.StartTimeline();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //到末尾了，转到第一帧，重新播放
                if (editorWarpMode == EditorWarpMode.Once && (Mathf.Approximately(currentTime, length)) || currentTime > length)
                {
                    currentTime = 0;
                }
            
                editorPlaybackState = EditorPlaybackState.PlayingForwards;
                editorPreviousTime = Time.realtimeSinceStartup;
                lastStartPlayTime = currentTime;
                OnStopInEditor = callback != null ? callback : OnStopInEditor;
            }
            // if ( Application.isPlaying ) {
            //     var temp = cutscene.currentTime == length ? 0 : cutscene.currentTime;
            //     cutscene.Play(0, length, cutscene.defaultWrapMode, callback, Cutscene.PlayingDirection.Forwards);
            //     cutscene.currentTime = temp;
            //     return;
            // }

        }

        //Play reverse button pressed
        public void PlayReverse() {


            /*if ( Application.isPlaying ) {
                var temp = cutscene.currentTime == 0 ? length : cutscene.currentTime;
                cutscene.Play(0, length, cutscene.defaultWrapMode, null, Cutscene.PlayingDirection.Backwards);
                cutscene.currentTime = temp;
                return;
            }

            editorPlaybackState = EditorPlaybackState.PlayingBackwards;
            editorPreviousTime = Time.realtimeSinceStartup;
            if ( cutscene.currentTime == 0 ) {
                cutscene.currentTime = length;
                lastStartPlayTime = 0;
            } else {
                lastStartPlayTime = cutscene.currentTime;
            }*/
        }

        //Pause button pressed
        public void Pause() {

            // if ( Application.isPlaying ) {
            //     if ( cutscene.isActive ) {
            //         cutscene.Pause();
            //         return;
            //     }
            // }

            editorPlaybackState = EditorPlaybackState.Stoped;
            if ( OnStopInEditor != null ) {
                OnStopInEditor();
                OnStopInEditor = null;
            }
        }

        //Stop button pressed or otherwise reset the scrubbing/previewing
        public void Stop(bool forceRewind) {
            // if ( Application.isPlaying ) {
            //     if ( cutscene.isActive ) {
            //         cutscene.Stop();
            //         return;
            //     }
            // }

            if ( OnStopInEditor != null ) {
                OnStopInEditor();
                OnStopInEditor = null;
            }

            //Super important to Sample instead of setting time here, so that we rewind correct if need be. 0 rewinds.
            //cutscene.Sample(editorPlaybackState != EditorPlaybackState.Stoped && !forceRewind ? lastStartPlayTime : 0);
            editorPlaybackState = EditorPlaybackState.Stoped;
            try
            {
                currentActionDirector?.EndTimeline();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            if (!forceRewind)
            {
                currentTime = 0;
            }
            willRepaint = true;
        }

        ///<summary>
        /// 改造这个了，这个现在功能是向前一帧
        /// Steps time forward to the next key time
        /// </summary>
        void StepForward(bool _ShowNotification = false) {
            if (Mathf.Approximately(currentTime, actionTimeline.GetLength()) ) {
                currentTime = 0;
                return;
            }
            currentTime = currentTime + 1f/FrameRate;
            if (_ShowNotification)
            {
                ShowNotification(new GUIContent("向前1帧"),0.2f);
            }
        }

        ///<summary>Steps time backwards to the previous key time</summary>
        void StepBackward(bool _ShowNotification = false) {
            if (currentTime == 0 ) {
                currentTime = actionTimeline.GetLength();
                return;
            }
            currentTime = currentTime - 1f/FrameRate;
            if (_ShowNotification)
            {
                ShowNotification(new GUIContent("向后1帧"),0.2f);
            }

        }
        
        //TODO
         void OnEditorUpdate() {
             //如果在Running的时候，进行修改应该把编辑器停下
            //当timeline资源为空
            if ( actionTimeline == null) 
            {
                return;
            }

            if ( EditorApplication.isCompiling )
            {
                Stop(true);
                return;
            }
            
            //Nothing.

            //TODO :
            //  添加速度参数
            var delta = ( Time.realtimeSinceStartup - editorPreviousTime ) * Time.timeScale;


            logicUpdateTimer += delta;
            editorPreviousTime = Time.realtimeSinceStartup;

            if (logicUpdateTimer >= 0.02f)
            {
                if (willRun)
                {
                    Play();
                    willRun = false;
                }
                
                OnEditorLogicUpdate();
                logicUpdateTimer -= 0.02f;
            }
            
            if (editorPlaybackState == EditorPlaybackState.Stoped) 
            {
                return;
            }
            //Sample at it's current time.
            //currentActionDirector.Sample(currentTime);


            //Playback.
            if (currentTime >= length && editorPlaybackState == EditorPlaybackState.PlayingForwards ) {
                //editorPlaybackWrapMode == Cutscene.WrapMode.Once
                if ( editorWarpMode == EditorWarpMode.Once) {
                    Stop(true);
                    return;
                }
                else
                {
                    Stop(true);
                    Play();
                    // //加一帧用来停止
                    // currentActionDirector.Sample(length+0.04f);
                     //currentActionDirector.Sample(0);
                     //currentActionDirector.Sample(delta);
                     //currentTime = 0;
                }
            }
            
            previousTime = currentTime;
            currentTime += editorPlaybackState == EditorPlaybackState.PlayingForwards ? delta : -delta;
        }

        void OnEditorLogicUpdate()
        {            
            if (editorPlaybackState == EditorPlaybackState.Stoped) 
            {
                return;
            }
           // currentActionDirector.LogicSample(currentTime);
        }

        //...
        void OnSceneGUI(SceneView sceneView) {

            if ( actionTimeline == null ) {
                return;
            }

            //Shortcuts for scene gui only
            var e = Event.current;
            if ( e.type == EventType.KeyDown ) {

                if ( e.keyCode == KeyCode.Space && !e.shift ) {
                    GUIUtility.keyboardControl = 0;
                    //if ( editorPlaybackState != EditorPlaybackState.Stoped ) { Stop(false); } else { Play(); }
                    e.Use();
                }

                if ( e.keyCode == KeyCode.RightArrow ) {
                    GUIUtility.keyboardControl = 0;
                    StepBackward(true);
                    e.Use();
                }

                if ( e.keyCode == KeyCode.LeftArrow ) {
                    GUIUtility.keyboardControl = 0;
                    StepForward(true);
                    e.Use();
                }
            }


            //Forward OnSceneGUI
            // if ( actionTimeline.directables != null ) {
            //     for ( var i = 0; i < cutscene.directables.Count; i++ ) {
            //         var directable = actionTimeline.directables[i];
            //         directable.SceneGUI(CutsceneUtility.selectedObject == directable);
            //     }
            // }
            //

            //No need to show tools of cutscene object, plus handles are shown per clip when required
            //Tools.hidden = ( Selection.activeObject == actionTimeline ) || Selection.activeObject is IDirectable || Selection.activeGameObject.GetComponents<ActionDirector>().Any();

            //Cutscene Root info and gizmos
            Handles.color = Prefs.gizmosColor;
            //Handles.Label(cutscene.transform.position + new Vector3(0, 0.4f, 0), "Cutscene Root");
            //Handles.DrawLine(cutscene.transform.position + cutscene.transform.forward, cutscene.transform.position + cutscene.transform.forward * -1);
            //Handles.DrawLine(cutscene.transform.position + cutscene.transform.right, cutscene.transform.position + cutscene.transform.right * -1);
            Handles.color = Color.white;
            
        }
        
          void OnGUI() {

            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            EditorStyles.label.richText = true;
            EditorStyles.textField.wordWrap = true;
            EditorStyles.foldout.richText = true;
            var e = Event.current;
            mousePosition = e.mousePosition;
            current = this;
            

            //avoid edit when compiling
            if ( EditorApplication.isCompiling ) {
                Stop(true);
                ShowNotification(new GUIContent("Compiling\n...Please wait..."));
                return;
            }

            //handle undo/redo shortcuts
            if ( e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed" ) {
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
               // multiSelection = null;
              // actionTimeline.Validate();
                InitClipWrappers();
                e.Use();
                return;
            }

            //prefab editing is not allowed
            //if ( isCutsceneAsset ) {
                //ShowNotification(new GUIContent("Editing Prefab Assets is not allowed\nPlease add an instance in the scene, or open the prefab for editing"));
              //  return;
           // }

            //remove notifications quickly
            if ( e.type == EventType.MouseDown ) { RemoveNotification(); }

            //button 2 seems buggy
            if ( e.button == 2 && e.type == EventType.MouseDown ) { isMouseButton2Down = true; }
            if ( e.button == 2 && e.rawType == EventType.MouseUp ) { isMouseButton2Down = false; }

            //Record Undo and dirty? This is an overal fallback. Certain actions register undo as well.
            var doRecordUndo = e.rawType == EventType.MouseDown && ( e.button == 0 || e.button == 1 );
            doRecordUndo |= e.type == EventType.DragPerform;
            if ( doRecordUndo ) {
               // Undo.RegisterFullObjectHierarchyUndo(cutscene.groupsRoot.gameObject, "Cutscene Change");
                //Undo.RecordObject(cutscene, "Cutscene Change");
                willDirty = true;
            }

            //reorder clips lists for better UI. This is strictly a UI thing.
            if ( interactingClip == null && e.type == EventType.Layout && actionTimeline ) {
                    foreach ( var track in actionTimeline.tracks ) {
                        if (track)
                        {
                            track.clips = track.clips.OrderBy(a => a.startTime).ToList();
                        }
                    }
            }

            //make the layout rects
            topLeftRect = new Rect(0, TOOLBAR_HEIGHT, LEFT_MARGIN, TOP_MARGIN);
            topMiddleRect = new Rect(LEFT_MARGIN, TOOLBAR_HEIGHT, screenWidth - LEFT_MARGIN - RIGHT_MARGIN, TOP_MARGIN);
            leftRect = new Rect(0, TOOLBAR_HEIGHT + TOP_MARGIN, LEFT_MARGIN, screenHeight - TOOLBAR_HEIGHT - TOP_MARGIN + scrollPos.y);
            centerRect = new Rect(LEFT_MARGIN, TOP_MARGIN + TOOLBAR_HEIGHT, screenWidth - LEFT_MARGIN - RIGHT_MARGIN, screenHeight - TOOLBAR_HEIGHT - TOP_MARGIN + scrollPos.y);

            //...
            DoKeyboardShortcuts();
            ShowPlaybackControls(topLeftRect);
            ShowTimeInfo(topMiddleRect);
            ShowToolbar();
            DoScrubControls();
            DoZoomAndPan();


            //Dirty and Resample flags?
            if ( e.rawType == EventType.MouseUp && e.button == 0 ) {
                willDirty = true;
                willResample = true;
            }


            //Timelines
            var scrollRect1 = Rect.MinMaxRect(0, centerRect.yMin, screenWidth, screenHeight - 5);
            var scrollRect2 = Rect.MinMaxRect(0, centerRect.yMin, screenWidth, totalHeight + 150);
            scrollPos = GUI.BeginScrollView(scrollRect1, scrollPos, scrollRect2);
            ShowGroupsAndTracksList(leftRect);
            ShowTimeLines(centerRect);
            GUI.EndScrollView();
            ///---

            DrawGuides();
            AcceptDrops();


            //Final stuff...

            //clean selection and hotcontrols
            if ( e.type == EventType.MouseDown && e.button == 0 && GUIUtility.hotControl == 0 ) {
                if ( centerRect.Contains(mousePosition) )
                {
                    Selection.activeObject = null;
                }
                GUIUtility.keyboardControl = 0;
                showDragDropInfo = false;
            }

            //just some info for the user to drag/drop gameobject in editor
            // if ( showDragDropInfo && cutscene.groups.Find(g => g.GetType() == typeof(ActorGroup)) == null ) {
            //     var label = "Drag & Drop GameObjects or Prefabs in this window to create Actor Groups";
            //     var size = new GUIStyle("label").CalcSize(new GUIContent(label));
            //     var notificationRect = new Rect(0, 0, size.x, size.y);
            //     notificationRect.center = new Vector2(( screenWidth / 2 ) + ( LEFT_MARGIN / 2 ), ( screenHeight / 2 ) + TOP_MARGIN);
            //     GUI.Label(notificationRect, label);
            // }

            //repaint?
            if ( e.type == EventType.MouseDrag || e.type == EventType.MouseUp || GUI.changed ) {
                willRepaint = true;
            }

            //dirty?
            if ( willDirty && actionTimeline) {
                willDirty = false;
                EditorUtility.SetDirty(actionTimeline);
            }

            //resample?
            // if ( willResample ) {
            //     willResample = false;
            //     //delaycall so that other gui controls are finalized before resample.
            //     EditorApplication.delayCall += () => { if ( cutscene != null ) cutscene.ReSample(); };
            // }

            //hack to show modal popup windows
            if ( onDoPopup != null ) {
                var temp = onDoPopup;
                onDoPopup = null;
                //QuickPopupQuickPopup.Show(temp);
            }

            //if a prefab darken whole UI
            // if ( isCutsceneAsset ) {
            //     GUI.color = Color.black.WithAlpha(0.5f);
            //     GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), whiteTexture);
            //     GUI.color = Color.white;
            // }

            //cheap ver/hor seperators
            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(0, centerRect.y + 1), new Vector2(centerRect.xMax, centerRect.y + 1));
            Handles.DrawLine(new Vector2(centerRect.x, centerRect.y + 1), new Vector2(centerRect.x, centerRect.yMax));
            Handles.color = Color.white;

            //repaint
            if ( willRepaint ) {
                willRepaint = false;
                Repaint();
            }

            //cleanup
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.skin = null;

            if ( viewTimeMax == 0 ) { GUI.Label(centerRect, "<size=40>:-)</size>", Styles.centerLabel); }
          }

          void DoKeyboardShortcuts()
          {
              var e = Event.current;
              if (e.type == EventType.KeyDown && GUIUtility.keyboardControl == 0 && !e.control && !e.shift)
              {
                  //play
                  if (e.keyCode == KeyCode.Space)
                  {
                      if (editorPlaybackState != EditorPlaybackState.Stoped)
                      {
                         // Stop(false);
                      }
                      else
                      {
                        //  Play();
                      }

                      e.Use();
                  }

                  //step forw
                  if (e.keyCode == KeyCode.RightArrow)
                  {
                      StepForward(true);
                      e.Use();
                  }

                  //step back
                  if (e.keyCode == KeyCode.LeftArrow)
                  {
                      StepBackward(true);
                      e.Use();
                  }

                  // //key at scrubber
                  // if (e.keyCode == KeyCode.K)
                  // {
                  //     var keyable = CutsceneUtility.selectedObject as IKeyable;
                  //     if (keyable != null)
                  //     {
                  //         keyable.TryAddIdentityKey(keyable.RootTimeToLocalTime());
                  //     }
                  //
                  //     e.Use();
                  // }

                  //split at scrubber
                  if (e.keyCode == KeyCode.S)
                  {
                     // var clip = CutsceneUtility.selectedObject as ActionClip;
                    //  if (clip != null)
                      {
                         // var wrapper = clipWrappersMap[clip];
                         // if (wrapper != null)
                          {
                            //  wrapper.Split(cutscene.currentTime);
                          }
                      }

                      e.Use();
                  }

                  //strech fit
                  if (e.keyCode == KeyCode.F)
                  {
                     // var clip = CutsceneUtility.selectedObject as ActionClip;
                      //if (clip != null)
                      {
                         // var wrapper = clipWrappersMap[clip];
                        //  if (wrapper != null)
                        //  {
                         //     wrapper.StretchFit();
                        //  }
                      }

                      e.Use();
                  }

                  //clean off range keys
                  if (e.keyCode == KeyCode.C)
                  {
                      // var clip = CutsceneUtility.selectedObject as ActionClip;
                      // if (clip != null)
                      // {
                      //     var wrapper = clipWrappersMap[clip];
                      //     if (wrapper != null)
                      //     {
                      //         wrapper.CleanKeysOffRange();
                      //     }
                      // }

                      e.Use();
                  }

                  //键盘删除操作
                  if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
                  {
                      /*if (multiSelection != null)
                      {
                          SafeDoAction(() =>
                          {
                              foreach (var act in multiSelection.Select(b => b.action).ToArray())
                              {
                                  (act.parent as CutsceneTrack).DeleteAction(act);
                              }

                              InitClipWrappers();
                              multiSelection = null;
                          });
                          e.Use();
                      }
                      else
                      {

                      }*/
                      var clip = Selection.activeObject as BaseActionClip;
                      if (clip != null)
                      {
                          SafeDoAction(() =>
                          {
                              (clip.parent as BaseActionTrack).DeleteAction(clip);
                              InitClipWrappers();
                              ShowNotification(new GUIContent("删除Clip"),1f);
                          });
                          e.Use();
                      }

                  }
                  

              }

              if (e.type == EventType.KeyDown && GUIUtility.keyboardControl == 0 && e.modifiers == EventModifiers.Control)
              {
                  //键盘复制操作
                  if (e.keyCode == KeyCode.C)
                  {
                      if (Selection.activeObject is BaseActionClip clip)
                      {
                          ActionTimelineUtility.CopyClip(clip); 
                          ShowNotification(new GUIContent($"复制Clip:[{clip.name}]"),1f);
                          e.Use();
                      }

                  }
                  
                  //键盘黏贴操作
                  if (e.keyCode == KeyCode.V)
                  {
                      if (Selection.activeObject is BaseActionTrack clip)
                      {
                          var result = ActionTimelineUtility.PasteClip(clip,currentTime);
                          if (result)
                          {
                              ShowNotification(new GUIContent($"黏贴Clip:[{result.name}]至Track:[{clip.name}]"),1f);
                          }
                          e.Use();
                      }

                  }
                  //键盘剪切操作
                  if (e.keyCode == KeyCode.X)
                  {
                      if (Selection.activeObject is BaseActionClip clip)
                      {
                          ShowNotification(new GUIContent($"剪切Clip:[{clip.name}]"),1f);
                          ActionTimelineUtility.CutClip(clip); 
                          e.Use();
                      }

                  }
              }

        }
          void DrawGuides() {

              //draw a vertical line at 0 time
              DrawGuideLine(0, isProSkin ? Color.white : Color.black);
              
              if(!actionTimeline)
                  return;
              
              //TODO
              //draw a vertical line at length time
              DrawGuideLine(actionTimeline.endTime, isProSkin ? Color.white : Color.black);



              //draw a vertical line at dragging clip start/end time
              //竖线，用于指示正在拖动的Clip
               if ( interactingClip != null ) {
                   if ( interactingClip.isDragging || interactingClip.isScalingStart ) {
                       DrawGuideLine(interactingClip.clip.startTime, Color.white.WithAlpha(0.05f));
                   }
                   if ( interactingClip.isDragging || interactingClip.isScalingEnd ) {
                       DrawGuideLine(interactingClip.clip.endTime, Color.white.WithAlpha(0.05f));
                   }
               }

              //active（Play状态下的目前到的红线）
              if (actionTimeline.isActive)
              {
                  DrawGuideLine(0, Color.red);
                  DrawGuideLine(actionTimeline.GetLength(), Color.red);
              }
              //draw a vertical line at current time
              if (currentTime > 0 ) {
                  DrawGuideLine(currentTime, scruberColor);
              }
              //draw other "subscribed" guidelines
              for ( var i = 0; i < pendingGuides.Count; i++ ) { DrawGuideLine(pendingGuides[i].time, pendingGuides[i].color); }
              pendingGuides.Clear();
          }
          
          //显示拖动的鼠标样式
          void AcceptDrops() {

              // if (currentTime > 0 ) {
              //     return;
              // }
              //
              // var e = Event.current;
              // if ( e.type == EventType.DragUpdated ) {
              //     DragAndDrop.visualMode = DragAndDropVisualMode.Link;
              // }

              /*if ( e.type == EventType.DragPerform ) {
                  for ( int i = 0; i < DragAndDrop.objectReferences.Length; i++ ) {
                      var o = DragAndDrop.objectReferences[i];
                      if ( o is GameObject ) {
                          var go = (GameObject)o;

                          if ( go.GetComponent<DirectorCamera>() != null ) {
                              ShowNotification(new GUIContent("The 'DIRECTOR' group is already used for the 'DirectorCamera' object"));
                              continue;
                          }

                          if ( cutscene.GetAffectedActors().Contains(go) ) {
                              ShowNotification(new GUIContent(string.Format("GameObject '{0}' is already in the cutscene", o.name)));
                              continue;
                          }

                          DragAndDrop.AcceptDrag();
                          var newGroup = cutscene.AddGroup<ActorGroup>(go);
                          newGroup.AddTrack<ActorActionTrack>("Action Track");
                          CutsceneUtility.selectedObject = newGroup;
                      }
                  }
              }*/
          }

          //乱七八糟的各种工具什么的
          void ShowToolbar()
          {
              if (!isProSkin)
              {
                  GUI.contentColor = Color.black.WithAlpha(0.7f);
              }

              //GUI.enabled = cutscene.currentTime <= 0;
              GUI.enabled = true;
              var e = Event.current;

              GUI.backgroundColor = Color.white;
              GUI.color = Color.white;
              GUILayout.BeginHorizontal(EditorStyles.toolbar);

              //选择哪个资源
              var next = EditorGUILayout.ObjectField(actionTimeline, typeof(ActionTimeline), false);
              ShowLinkCursor();
              if (next != actionTimeline)
              {
                  InitializeAll((ActionTimeline)next);
              }
              //将其显示在Inspector窗口，并Ping一下
              if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(60)))
              {
                  Selection.activeObject = actionTimeline;
                  EditorGUIUtility.PingObject(actionTimeline);
              }
              ShowLinkCursor();
              
              //显示现在是什么播放模式
              //TODO
              // 添加图标提示
              Texture warpModeTexture = editorWarpMode switch
              {
                  EditorWarpMode.Once => Styles.rippleIcon,
                  EditorWarpMode.Loop => Styles.loopIcon,
                  _ => throw new ArgumentOutOfRangeException()
              };
              if (GUILayout.Button(new GUIContent($"{editorWarpMode}",warpModeTexture), EditorStyles.toolbarButton,GUILayout.Width(60)))
              {
                  editorWarpMode =(EditorWarpMode)(((int)editorWarpMode + 1)%(Enum.GetValues(typeof(EditorWarpMode)).Length));
              }
              ShowLinkCursor();
              
              //只是显示帧率
              GUILayout.Label($"{FrameRate} FPS", EditorStyles.toolbarButton, GUILayout.Width(60));

              GUILayout.Space(10);

              //磁铁吸附
              Prefs.magnetSnapping = GUILayout.Toggle(Prefs.magnetSnapping, new GUIContent(Styles.magnetIcon, "Clips Magnet Snapping"),
                  EditorStyles.toolbarButton);
              ShowLinkCursor();
              //不懂
              Prefs.rippleMode = GUILayout.Toggle(Prefs.rippleMode, new GUIContent(Styles.rippleIcon, "Ripple moving of next clips and keyframes (Shift)"),
                  EditorStyles.toolbarButton);
              ShowLinkCursor();
              //重定时，感觉没什么用，改成回到最长长度吧
              // Prefs.retimeMode = GUILayout.Toggle(Prefs.retimeMode, new GUIContent(Styles.retimeIcon, "Retiming of keyframes when scaling clips"),
              //     EditorStyles.toolbarButton);
              if (GUILayout.Button( new GUIContent(Styles.retimeIcon, "将时间轴与Timeline整体同步"),EditorStyles.toolbarButton))
              {
                  UpdateViewRange();
              }
              ShowLinkCursor();
              GUILayout.Space(10);

              GUILayout.FlexibleSpace();
              //TODO
              // 自动key帧吧，还没弄明白
              if (!Prefs.autoKey)
              {
                  var wasEnabled = GUI.enabled;
                  GUI.enabled = true;
                  var changedParams = ActionTimelineUtility.changedParameterCallbacks;
                  var hasChangedParams = changedParams != null && changedParams.Count > 0;
                  GUI.color = hasChangedParams ? Color.white : Color.clear;
                  GUILayout.BeginHorizontal();
                  if (hasChangedParams)
                  {
                      GUI.backgroundColor = Color.clear;
                      GUI.color = Color.green;
                      var b1 = GUILayout.Button(Styles.keyIcon, EditorStyles.toolbarButton);
                      GUI.color = Color.white;
                      var b2 = GUILayout.Button(string.Format("Key ({0}) Changed Parameters", changedParams.Count), EditorStyles.toolbarButton);
                      GUI.backgroundColor = Color.white;
                      if (b1 || b2)
                      {
                          foreach (var pair in changedParams)
                          {
                              pair.Value.Commit();
                          }
                      }
                  }

                  GUI.color = Color.white;
                  GUILayout.EndHorizontal();
                  GUI.enabled = wasEnabled;
              }

              GUILayout.FlexibleSpace();

              //要导入的模型或者之类的
              var nextActingPrefab = EditorGUILayout.ObjectField(actingPrefab, typeof(GameObject), true);
              //TODO
              // 这里要有额外的判断，判断是场景还是预制体，如果是预制体，切换到编辑器场景，否则就在原地播放
              if (nextActingPrefab != actingPrefab)
              {
                  actingPrefab = (GameObject)nextActingPrefab;
              }
              ShowLinkCursor();
              
              GUI.color = Color.white;
              GUILayout.EndHorizontal();
              GUI.enabled = true;
              GUI.backgroundColor = Color.white;
          }

          //拉动时间点
          void DoScrubControls()
          {
              
              //只有在正在播放的时候才不显示
              if (( actionTimeline?.isActive ).GetValueOrDefault(false)) { //no scrubbing if playing in runtime
                  return;
              }
              //
              var e = Event.current;
              if ( e.type == EventType.MouseDown && topMiddleRect.Contains(mousePosition) ) {
                  var carretPos = TimeToPos(length) + leftRect.width;
                  var isEndCarret = e.control;
                  if ( isEndCarret ) { CacheMagnetSnapTimes(); }


                  GUI.DrawTexture(LengthTragRect, Styles.carretIcon);
                  if (GUIUtility.ScreenToGUIRect(LengthTragRect).Contains(mousePosition) || isEndCarret) 
                  {
                      if ( e.button == 0) {
                          isMovingEndCarret = true;
                          isMovingScrubCarret = false;
                          //最终时间更改了，所以需要停下
                          Pause();
                      }

                      if (e.button ==1 && actionTimeline)
                      {
                          var menu = new GenericMenu();
                          menu.AddItem(new GUIContent("设置时间至最后一个Clip结束"), false, () =>
                          {
                              var lastClip = actionTimeline.GetAllDirectables().Where(d => d is BaseActionClip).OrderBy(d => d.endTime).LastOrDefault();
                              if (lastClip != null)
                              {
                                  length = lastClip.endTime;
                              }
                          });
                          menu.ShowAsContext();
                      }
                  }
                  else
                  {
                      if ( e.button == 0) {
                          isMovingScrubCarret = true;
                      }

                  }
                  

                  // if ( e.button == 1 && isEndCarret && actionTimeline.directables != null ) {
                  //     var menu = new GenericMenu();
                  //     menu.AddItem(new GUIContent("Set To Last Clip Time"), false, () =>
                  //         {
                  //             var lastClip = actionTimeline.directables.Where(d => d is ActionClip).OrderBy(d => d.endTime).LastOrDefault();
                  //             if ( lastClip != null ) {
                  //                 length = lastClip.endTime;
                  //             }
                  //         });
                  //     menu.ShowAsContext();
                  // }

                  e.Use();
              }

              if ( e.button == 0 && e.rawType == EventType.MouseUp ) {
                  isMovingScrubCarret = false;
                  isMovingEndCarret = false;
              }

              var pointerTime = PosToTime(mousePosition.x);
              if ( isMovingScrubCarret ) {
                 currentTime = SnapTime(pointerTime);
                 currentTime = Mathf.Clamp(currentTime, Mathf.Max(viewTimeMin, 0) + float.Epsilon, length - float.Epsilon);
                 var range = new Rect()
                 {
                     position = Vector2.zero,
                     size = position.size
                 };
                 EditorGUIUtility.AddCursorRect(range,MouseCursor.MoveArrow);
              }

              if ( isMovingEndCarret ) {
                  length = SnapTime(pointerTime);
                  var magnetSnap = MagnetSnapTime(length, magnetSnapTimesCache);
                  length = magnetSnap != null ? magnetSnap.Value : length;
                  length = Mathf.Clamp(length, viewTimeMin + float.Epsilon, viewTimeMax - float.Epsilon);
                  var range = new Rect()
                  {
                      position = Vector2.zero,
                      size = position.size
                  };
                  EditorGUIUtility.AddCursorRect(range,MouseCursor.MoveArrow);
              }
          }
          
          
          void DoZoomAndPan() {

              if ( !centerRect.Contains(mousePosition) ) {
                  return;
              }

              var e = Event.current;
              //Zoom or scroll down/up if prefs is set to scrollwheel
              if ( ( e.type == EventType.ScrollWheel && Prefs.scrollWheelZooms ) || ( e.alt && !e.shift && e.button == 1 ) ) {
                  this.AddCursorRect(centerRect, MouseCursor.Zoom);
                  if ( e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.ScrollWheel ) {
                      var pointerTimeA = PosToTime(mousePosition.x);
                      var delta = e.alt ? -e.delta.x * 0.1f : e.delta.y;
                      var t = ( Mathf.Abs(delta * 25) / centerRect.width ) * viewTime;
                      viewTimeMin += delta > 0 ? -t : t;
                      viewTimeMax += delta > 0 ? t : -t;
                      var pointerTimeB = PosToTime(mousePosition.x + e.delta.x);
                      var diff = pointerTimeA - pointerTimeB;
                      viewTimeMin += diff;
                      viewTimeMax += diff;
                      e.Use();
                  }
              }

              //pan left/right, up/down
              if ( isMouseButton2Down || ( e.alt && !e.shift && e.button == 0 ) ) {
                  this.AddCursorRect(centerRect, MouseCursor.Pan);
                  if ( e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseUp ) {
                      var t = ( Mathf.Abs(e.delta.x) / centerRect.width ) * viewTime;
                      viewTimeMin += e.delta.x > 0 ? -t : t;
                      viewTimeMax += e.delta.x > 0 ? -t : t;
                      scrollPos.y -= e.delta.y;
                      e.Use();
                  }
              }
          }


          //top left controls
          void ShowPlaybackControls(Rect topLeftRect)
          {
              var autoKeyRect = new Rect(topLeftRect.xMin + 10, topLeftRect.yMin + 4, 32, 32);
              AddCursorRect(autoKeyRect, MouseCursor.Link);
              GUI.backgroundColor = Prefs.autoKey ? Color.black.WithAlpha(0.5f) : Color.grey.WithAlpha(0.5f);
              GUI.Box(autoKeyRect, string.Empty, Styles.clipBoxStyle);
              GUI.color = Prefs.autoKey ? new Color(1, 0.4f, 0.4f) : Color.white;
              GUI.backgroundColor = Color.clear;
              if (GUI.Button(autoKeyRect, Styles.keyIcon, (GUIStyle)"box"))
              {
                  Prefs.autoKey = !Prefs.autoKey;
                  ShowNotification(new GUIContent(string.Format("AutoKey {0}", Prefs.autoKey ? "Enabled" : "Disabled"), Styles.keyIcon));
              }

              var autoKeyLabelRect = autoKeyRect;
              autoKeyLabelRect.yMin += 16;
              GUI.backgroundColor = Color.white;
              GUI.Label(autoKeyLabelRect, "<color=#AAAAAA>Auto</color>", Styles.centerLabel);
              GUI.color = Color.white;


              if (!isProSkin)
              {
                  GUI.contentColor = Color.black.WithAlpha(0.7f);
              }

              //Cutscene shows the gui
              GUILayout.BeginArea(topLeftRect);

              GUILayout.BeginVertical();
              GUILayout.FlexibleSpace();

              GUILayout.BeginHorizontal();
              GUILayout.FlexibleSpace();

              Rect lastRect;
              if (GUILayout.Button(Styles.stepReverseIcon, (GUIStyle)"box", GUILayout.Width(20), GUILayout.Height(20)))
              {
                  StepBackward();
                  Event.current.Use();
              }

              lastRect = GUILayoutUtility.GetLastRect();
              if (lastRect.Contains(Event.current.mousePosition))
              {
                  AddCursorRect(lastRect, MouseCursor.Link);
              }


              var isStoped =   editorPlaybackState == EditorPlaybackState.Stoped;
             //Application.isPlaying ? (cutscene.isPaused || !cutscene.isActive) : editorPlaybackState == EditorPlaybackState.Stoped;
              if (isStoped)
              {
                  if (GUILayout.Button(Styles.playIcon, (GUIStyle)"box", GUILayout.Width(20), GUILayout.Height(20)))
                  {
                      willRun = true;
                      //Play();
                      Event.current.Use();
                  }

                  lastRect = GUILayoutUtility.GetLastRect();
                  if (lastRect.Contains(Event.current.mousePosition))
                  {
                      AddCursorRect(lastRect, MouseCursor.Link);
                  }
              }
              else
              {
                  if (GUILayout.Button(Styles.pauseIcon, (GUIStyle)"box", GUILayout.Width(20), GUILayout.Height(20)))
                  {
                      Pause();
                      Event.current.Use();
                  }

                  lastRect = GUILayoutUtility.GetLastRect();
                  if (lastRect.Contains(Event.current.mousePosition))
                  {
                      AddCursorRect(lastRect, MouseCursor.Link);
                  }
              }


              if (GUILayout.Button(Styles.stopIcon, (GUIStyle)"box", GUILayout.Width(20), GUILayout.Height(20)))
              {
                  Stop(false);
                  Event.current.Use();
              }

              lastRect = GUILayoutUtility.GetLastRect();
              if (lastRect.Contains(Event.current.mousePosition))
              {
                  AddCursorRect(lastRect, MouseCursor.Link);
              }

              if (GUILayout.Button(Styles.stepIcon, (GUIStyle)"box", GUILayout.Width(20), GUILayout.Height(20)))
              {
                  StepForward();
                  Event.current.Use();
              }

              lastRect = GUILayoutUtility.GetLastRect();
              if (lastRect.Contains(Event.current.mousePosition))
              {
                  AddCursorRect(lastRect, MouseCursor.Link);
              }

              GUI.backgroundColor = Color.white;

              GUILayout.FlexibleSpace();
              GUILayout.EndHorizontal();

              GUILayout.FlexibleSpace();
              GUILayout.EndVertical();

              GUILayout.EndArea();

              GUI.contentColor = Color.white;
          }


          //top mid - viewTime selection and time info
          void ShowTimeInfo(Rect topMiddleRect)
          {
              GUI.color = Color.white.WithAlpha(0.2f);
              GUI.Box(topMiddleRect, string.Empty, EditorStyles.toolbarButton);
              GUI.color = Color.black.WithAlpha(0.2f);
              GUI.Box(topMiddleRect, string.Empty, Styles.timeBoxStyle);
              GUI.color = Color.white;

              timeInfoInterval = 1000000f;
              timeInfoHighMod = timeInfoInterval;
              var lowMod = 0.01f;
              var modulos = new float[] { 0.1f, 0.5f, 1, 5, 10, 50, 100, 500, 1000, 5000, 10000, 50000, 100000, 250000, 500000 }; //... O.o
              for (var i = 0; i < modulos.Length; i++)
              {
                  var count = viewTime / modulos[i];
                  if (centerRect.width / count > 50)
                  {
                      //50 is approx width of label
                      timeInfoInterval = modulos[i];
                      lowMod = i > 0 ? modulos[i - 1] : lowMod;
                      timeInfoHighMod = i < modulos.Length - 1 ? modulos[i + 1] : timeInfoHighMod;
                      break;
                  }
              }

              var doFrames = Prefs.timeStepMode == Prefs.TimeStepMode.Frames;
              var timeStep = doFrames ? (1f / Prefs.frameRate) : lowMod;

              timeInfoStart = (float)Mathf.FloorToInt(viewTimeMin / timeInfoInterval) * timeInfoInterval;
              timeInfoEnd = (float)Mathf.CeilToInt(viewTimeMax / timeInfoInterval) * timeInfoInterval;
              timeInfoStart = Mathf.Round(timeInfoStart * 10) / 10;
              timeInfoEnd = Mathf.Round(timeInfoEnd * 10) / 10;

              GUI.BeginGroup(topMiddleRect);
              {
                  //the minMax slider
                  var _timeMin = viewTimeMin;
                  var _timeMax = viewTimeMax;
                  var sliderRect = new Rect(5, 0, topMiddleRect.width - 10, 18);
                  EditorGUI.MinMaxSlider(sliderRect, ref _timeMin, ref _timeMax, 0, maxTime);
                  viewTimeMin = _timeMin;
                  viewTimeMax = _timeMax;
                  if (sliderRect.Contains(Event.current.mousePosition) && Event.current.clickCount == 2)
                  {
                      viewTimeMin = 0;
                      viewTimeMax = length;
                  }

                  GUI.color = Color.white.WithAlpha(0.1f);
                  GUI.DrawTexture(Rect.MinMaxRect(0, TOP_MARGIN - 1, topMiddleRect.xMax, TOP_MARGIN), Styles.whiteTexture);
                  GUI.color = Color.white;

                  //the step interval
                  if (centerRect.width / (viewTime / timeStep) > 6)
                  {
                      for (var i = timeInfoStart; i <= timeInfoEnd; i += timeStep)
                      {
                          var posX = TimeToPos(i);
                          var frameRect = Rect.MinMaxRect(posX - 1, TOP_MARGIN - 2, posX + 1, TOP_MARGIN - 1);
                          GUI.color = isProSkin ? Color.white : Color.black;
                          GUI.DrawTexture(frameRect, whiteTexture);
                          GUI.color = Color.white;
                      }
                  }

                  //the time interval
                  for (var i = timeInfoStart; i <= timeInfoEnd; i += timeInfoInterval)
                  {
                      var posX = TimeToPos(i);
                      var rounded = Mathf.Round(i * 10) / 10;

                      GUI.color = isProSkin ? Color.white : Color.black;
                      var markRect = Rect.MinMaxRect(posX - 2, TOP_MARGIN - 3, posX + 2, TOP_MARGIN - 1);
                      GUI.DrawTexture(markRect, whiteTexture);
                      GUI.color = Color.white;

                      var text = doFrames ? (rounded * Prefs.frameRate).ToString("0") : rounded.ToString("0.00");
                      var size = GUI.skin.GetStyle("label").CalcSize(new GUIContent(text));
                      var stampRect = new Rect(0, 0, size.x, size.y);
                      stampRect.center = new Vector2(posX, TOP_MARGIN - size.y + 4);
                      GUI.color = rounded % timeInfoHighMod == 0 ? Color.white : Color.white.WithAlpha(0.5f);
                      GUI.Box(stampRect, text, (GUIStyle)"label");
                      GUI.color = Color.white;
                  }

                  //the number showing current time when scubing
                  if (currentTime > 1f/ FrameRate || Mathf.Approximately(currentTime,1f/FrameRate))
                  {
                      var label = doFrames ? (currentTime * Prefs.frameRate).ToString("0") : currentTime.ToString("0.00");
                      var text = "<b><size=17>" + label + "</size></b>";
                      var size = Styles.headerBoxStyle.CalcSize(new GUIContent(text));
                      var posX = TimeToPos(currentTime);
                      var stampRect = new Rect(0, 0, size.x, size.y);
                      stampRect.center = new Vector2(posX, TOP_MARGIN - size.y / 2);

                      GUI.backgroundColor = isProSkin ? Color.black.WithAlpha(0.4f) : Color.black.WithAlpha(0.7f);
                      GUI.color = scruberColor;
                      GUI.Box(stampRect, text, Styles.headerBoxStyle);
                  }

                  //the length position carret texture and pre-exit length indication
                  //最终长度的位置，以及上面的三角形
                  var lengthPos = TimeToPos(length);
                  var lengthRect = new Rect(0, 0, 16, 16);
                  lengthRect.center = new Vector2(lengthPos, TOP_MARGIN - 2);
                  LengthTragRect = GUIUtility.GUIToScreenRect(lengthRect);
                  GUI.color = isProSkin ? Color.white : Color.black;
                  GUI.DrawTexture(lengthRect, Styles.carretIcon);
                  GUI.color = Color.white;
              }
              GUI.EndGroup();
          }

          private bool OpenAddTrackSubWindow;
          void ShowGroupsAndTracksList(Rect leftRect)
          {
              var e = Event.current;

              //allow resize list width
              var scaleRect = new Rect(leftRect.xMax - 4, leftRect.yMin, 4, leftRect.height);
              AddCursorRect(scaleRect, MouseCursor.ResizeHorizontal);
              if (e.type == EventType.MouseDown && e.button == 0 && scaleRect.Contains(e.mousePosition))
              {
                  isResizingLeftMargin = true;
                  e.Use();
              }

              if (isResizingLeftMargin)
              {
                  LEFT_MARGIN = e.mousePosition.x + 2;
              }

              if (e.rawType == EventType.MouseUp)
              {
                  isResizingLeftMargin = false;
              }
              GUI.enabled = true;
              //GUI.enabled = cutscene.currentTime <= 0;

              //starting height && search.
              var nextYPos = FIRST_GROUP_TOP_MARGIN;
              var wasEnabled = GUI.enabled;
              GUI.enabled = true;
              var collapseAllRect = Rect.MinMaxRect(leftRect.x + 5, leftRect.y + 4, 20, leftRect.y + 20 - 1);
              var searchRect = Rect.MinMaxRect(leftRect.x + 20, leftRect.y + 4, leftRect.xMax - 18, leftRect.y + 20 - 1);
              var searchCancelRect = Rect.MinMaxRect(searchRect.xMax, searchRect.y, leftRect.xMax - 4, searchRect.yMax);
              var anyExpanded = true;
              //var anyExpanded = cutscene.groups.Any(g => !g.isCollapsed);
              AddCursorRect(collapseAllRect, MouseCursor.Link);
              GUI.color = Color.white.WithAlpha(0.5f);

              GUI.color = Color.white;
#if UNITY_2022_3_OR_NEWER
              var searchFieldText = (GUIStyle)"ToolbarSearchTextField";
              var searchFieldCancel = (GUIStyle)"ToolbarSearchCancelButton";
#else
            var searchFieldText = (GUIStyle)"ToolbarSeachTextField";
            var searchFieldCancel = (GUIStyle)"ToolbarSeachCancelButton";
#endif
              searchString = EditorGUI.TextField(searchRect, searchString, searchFieldText);
              if (GUI.Button(searchCancelRect, string.Empty, searchFieldCancel))
              {
                  searchString = string.Empty;
                  GUIUtility.keyboardControl = 0;
              }

              GUI.enabled = wasEnabled;


              //begin area for left Rect
              GUI.BeginGroup(leftRect);
              ShowListTracks(e, ref nextYPos);
              GUI.EndGroup();

              //store total height required
              totalHeight = nextYPos;


              //Simple button to add empty group for convenience
              var addButtonY = totalHeight + TOP_MARGIN + TOOLBAR_HEIGHT + 20;
              var addRect = Rect.MinMaxRect(leftRect.xMin + 10, addButtonY, leftRect.xMax - 10, addButtonY + 20);
              GUI.color = Color.white;
              var addTrackRect = Rect.MinMaxRect(addRect.xMin, addRect.yMax, addRect.xMax, addRect.yMax +ADDTRACK_GROUP_HEIGHT);
              if (e.rawType == EventType.MouseDown && e.button == 0&&!addTrackRect.Contains(e.mousePosition) && OpenAddTrackSubWindow)
              {
                  OpenAddTrackSubWindow = false;
              }
              
              if (GUI.Button(addRect, "AddTrack",EditorStyles.miniButtonMid))
              {
                  OpenAddTrackSubWindow = true;
                  //var newTrack = actionTimeline.AddTrack(typeof(BaseActionTrack),nameof(BaseActionTrack));
                  //CutsceneUtility.selectedObject = newGroup;
              }

              if (OpenAddTrackSubWindow)
              {
                  totalHeight += ADDTRACK_GROUP_HEIGHT;
                  var area = Rect.MinMaxRect(0,0,addTrackRect.width,addTrackRect.height);
                  GUI.color = Color.gray;
                  GUI.Box(addTrackRect,"");
                  GUI.BeginGroup(addTrackRect);
                  ShowAddTrackSubWindow(area);
                  GUI.EndGroup();
              }


              
              GUI.enabled = true;
              GUI.color = Color.white;
          }
          private Vector2 _addTrackScrollPos;
          void ShowAddTrackSubWindow(Rect addTrackRect)
          {
              var searchRect = Rect.MinMaxRect(addTrackRect.x+4, addTrackRect.y, addTrackRect.xMax - 4, addTrackRect.y + 20 - 1);
              
              GUI.color = Color.white;
#if UNITY_2022_3_OR_NEWER
              var searchFieldText = (GUIStyle)"ToolbarSearchTextField";
              var searchFieldCancel = (GUIStyle)"ToolbarSearchCancelButton";
#else
            var searchFieldText = (GUIStyle)"ToolbarSeachTextField";
            var searchFieldCancel = (GUIStyle)"ToolbarSeachCancelButton";
#endif

              addTrackSearchString = EditorGUI.TextField(searchRect, addTrackSearchString, searchFieldText);
              

              // 过滤组件
               var filteredTypes = TypeCache.GetTypesDerivedFrom<BaseActionTrack>().ToArray()
                   .Where(t =>
                   {
                       bool uniqueTest = true;
                       if (t.RTGetAttribute<UniqueElementAttribute>(true) != null)
                       {
                           uniqueTest = !actionTimeline.tracks.Find((track) => track.GetType() == t);
                       }
                       return t != null && t.Name.Contains(addTrackSearchString) && uniqueTest;
                   })
                   .ToList();
               filteredTypes.Add(typeof(BaseActionTrack));
               
               
               
              // 滚动列表
              EditorGUILayout.BeginVertical();
              _addTrackScrollPos = EditorGUILayout.BeginScrollView(_addTrackScrollPos,GUILayout.Width(addTrackRect.width),GUILayout.Height(addTrackRect.height));
              var style = new GUIStyle(EditorStyles.toolbarButton);
              style.alignment = TextAnchor.MiddleLeft;
              foreach (var type in filteredTypes)
              {
                  if (GUILayout.Button(new GUIContent(type.Name,GetTrackIcon(type)), style,
                          GUILayout.Width(addTrackRect.width)))
                  {
                      actionTimeline.AddTrack(type);
                      OpenAddTrackSubWindow = false;
                  }
              }
              EditorGUILayout.EndScrollView();
              EditorGUILayout.EndVertical();
          }

          Texture GetTrackIcon(Type type)
          {
              Texture _icon = null;
              var att = type.RTGetAttribute<IconAttribute>(true);
              if ( att != null ) {
                  _icon = Resources.Load(att.iconName) as Texture;
                  if ( _icon == null && !string.IsNullOrEmpty(att.iconName) ) {
                      _icon = UnityEditor.EditorGUIUtility.FindTexture(att.iconName) as Texture;
                  }
                  if ( _icon == null && att.fromType != null ) {
                      _icon = UnityEditor.AssetPreview.GetMiniTypeThumbnail(att.fromType);
                  }
              }
              return _icon as Texture;
          }
          
          //具体显示Track的地方
          /*void ShowListGroups(Event e, ref float nextYPos)
          {
              //GROUPS
              for (int g = 0; g < cutscene.groups.Count; g++)
              {
                  var group = cutscene.groups[g];

                  if (IsFilteredOutBySearch(group, searchString))
                  {
                      group.isCollapsed = true;
                      continue;
                  }

                  var groupRect = new Rect(4, nextYPos, leftRect.width - GROUP_RIGHT_MARGIN - 4, GROUP_HEIGHT - 3);
                  this.AddCursorRect(groupRect, pickedGroup == null ? MouseCursor.Link : MouseCursor.MoveArrow);
                  nextYPos += GROUP_HEIGHT;

                  //highligh?
                  var groupSelected = (ReferenceEquals(group, CutsceneUtility.selectedObject) || group == pickedGroup);
                  GUI.color = groupSelected ? LIST_SELECTION_COLOR : GROUP_COLOR;
                  GUI.Box(groupRect, string.Empty, Styles.headerBoxStyle);
                  GUI.color = Color.white;


                  //GROUP CONTROLS
                  var plusClicked = false;
                  GUI.color = isProSkin ? Color.white.WithAlpha(0.5f) : new Color(0.2f, 0.2f, 0.2f);
                  var plusRect = new Rect(groupRect.xMax - 14, groupRect.y + 5, 8, 8);
                  if (GUI.Button(plusRect, Slate.Styles.plusIcon, GUIStyle.none))
                  {
                      plusClicked = true;
                  }

                  if (!group.isActive)
                  {
                      var disableIconRect = new Rect(plusRect.xMin - 20, groupRect.y + 1, 16, 16);
                      if (GUI.Button(disableIconRect, Styles.hiddenIcon, GUIStyle.none))
                      {
                          group.isActive = true;
                      }
                  }

                  if (group.isLocked)
                  {
                      var lockIconRect = new Rect(plusRect.xMin - (group.isActive ? 20 : 36), groupRect.y + 1, 16, 16);
                      if (GUI.Button(lockIconRect, Styles.lockIcon, GUIStyle.none))
                      {
                          group.isLocked = false;
                      }
                  }

                  GUI.color = isProSkin ? Color.yellow : Color.white;
                  GUI.color = group.isActive ? GUI.color : Color.grey;
                  var foldRect = new Rect(groupRect.x + 2, groupRect.y + 1, 20, groupRect.height);
                  var isVirtual = group.referenceMode == CutsceneGroup.ActorReferenceMode.UseInstanceHideOriginal;
                  group.isCollapsed = !EditorGUI.Foldout(foldRect, !group.isCollapsed,
                      string.Format("<b>{0} {1}</b>", group.name, isVirtual ? "(Ref)" : string.Empty));
                  GUI.color = Color.white;
                  //Actor Object Field
                  if (group.actor == null)
                  {
                      var oRect = Rect.MinMaxRect(groupRect.xMin + 20, groupRect.yMin + 1, groupRect.xMax - 20, groupRect.yMax - 1);
                      group.actor = (GameObject)UnityEditor.EditorGUI.ObjectField(oRect, group.actor, typeof(GameObject), true);
                  }
                  ///---

                  //CONTEXT
                  if ((e.type == EventType.ContextClick && groupRect.Contains(e.mousePosition)) || plusClicked)
                  {
                      var menu = new GenericMenu();
                      foreach (var _info in EditorTools.GetTypeMetaDerivedFrom(typeof(CutsceneTrack)))
                      {
                          var info = _info;
                          if (info.attachableTypes == null || !info.attachableTypes.Contains(group.GetType()))
                          {
                              continue;
                          }

                          var canAdd = !info.isUnique || (group.tracks.Find(track => track.GetType() == info.type) == null);
                          var finalPath = string.IsNullOrEmpty(info.category) ? info.name : info.category + "/" + info.name;
                          if (canAdd)
                          {
                              menu.AddItem(new GUIContent("Add Track/" + finalPath), false, () => { group.AddTrack(info.type); });
                          }
                          else
                          {
                              menu.AddDisabledItem(new GUIContent("Add Track/" + finalPath));
                          }
                      }

                      if (group.CanAddTrack(copyTrack))
                      {
                          menu.AddItem(new GUIContent("Paste Track"), false, () => { group.DuplicateTrack(copyTrack); });
                      }
                      else
                      {
                          menu.AddDisabledItem(new GUIContent("Paste Track"));
                      }

                      menu.AddItem(new GUIContent("Disable Group"), !group.isActive, () => { group.isActive = !group.isActive; });
                      menu.AddItem(new GUIContent("Lock Group"), group.isLocked, () => { group.isLocked = !group.isLocked; });

                      if (!(group is DirectorGroup))
                      {
                          menu.AddItem(new GUIContent("Select Actor (Double Click)"), false, () => { Selection.activeObject = group.actor; });
                          menu.AddItem(new GUIContent("Replace Actor"), false, () => { group.actor = null; });
                          menu.AddItem(new GUIContent("Duplicate"), false, () =>
                          {
                              cutscene.DuplicateGroup(group);
                              InitClipWrappers();
                          });
                          menu.AddSeparator("/");
                          menu.AddItem(new GUIContent("Delete Group"), false, () =>
                          {
                              if (EditorUtility.DisplayDialog("Delete Group", "Are you sure?", "YES", "NO!"))
                              {
                                  cutscene.DeleteGroup(group);
                                  InitClipWrappers();
                              }
                          });
                      }

                      menu.ShowAsContext();
                      e.Use();
                  }


                  //REORDERING
                  if (e.type == EventType.MouseDown && e.button == 0 && groupRect.Contains(e.mousePosition))
                  {
                      CutsceneUtility.selectedObject = group;
                      if (!(group is DirectorGroup))
                      {
                          pickedGroup = group;
                      }

                      if (e.clickCount == 2)
                      {
                          Selection.activeGameObject = group.actor;
                      }

                      e.Use();
                  }

                  if (pickedGroup != null && pickedGroup != group && !(group is DirectorGroup))
                  {
                      if (groupRect.Contains(e.mousePosition))
                      {
                          var markRect = new Rect(groupRect.x, (cutscene.groups.IndexOf(pickedGroup) < g) ? groupRect.yMax - 2 : groupRect.y, groupRect.width,
                              2);
                          GUI.color = Color.grey;
                          GUI.DrawTexture(markRect, Styles.whiteTexture);
                          GUI.color = Color.white;
                      }

                      if (e.rawType == EventType.MouseUp && e.button == 0 && groupRect.Contains(e.mousePosition))
                      {
                          cutscene.groups.Remove(pickedGroup);
                          cutscene.groups.Insert(g, pickedGroup);
                          cutscene.Validate();
                          pickedGroup = null;
                          e.Use();
                      }
                  }

                  //SHOW TRACKS (?)
                  if (!group.isCollapsed)
                  {
                      ShowListTracks(e, group, ref nextYPos);
                      //draw vertical graphic on left side of nested track rects
                      GUI.color = groupSelected ? LIST_SELECTION_COLOR : GROUP_COLOR;
                      var verticalRect = Rect.MinMaxRect(groupRect.x, groupRect.yMax, groupRect.x + 3, nextYPos - 2);
                      GUI.DrawTexture(verticalRect, Styles.whiteTexture);
                      GUI.color = Color.white;
                  }
              }
        }*/
          private bool isPressed;
          void ShowListTracks(Event e, ref float nextYPos)
          {
              if(!actionTimeline)
                  return;
              //TRACKS
              for (int t = 0; t < actionTimeline.tracks.Count; t++)
              {
                  var track = actionTimeline.tracks[t];
                  var yPos = nextYPos;

                  var trackRect = new Rect(10, yPos, leftRect.width - TRACK_RIGHT_MARGIN - 10, track.finalHeight);
                  nextYPos += track.finalHeight + TRACK_MARGINS;

                  //GRAPHICS
                  GUI.color = ColorUtility.Grey(isProSkin ? (track.isActive ? 0.25f : 0.2f) : (track.isActive ? 0.9f : 0.8f));
                  GUI.DrawTexture(trackRect, whiteTexture);
                  GUI.color = Color.white.WithAlpha(0.25f);
                  GUI.Box(trackRect, string.Empty, (GUIStyle)"flow node 0");
                  if (track == pickedTrack && track == Selection.activeObject)
                  {
                      GUI.color = LIST_SELECTION_COLOR;
                      GUI.DrawTexture(trackRect, whiteTexture);
                  }

                  //custom color indicator
                  if (track.isActive && track.color != Color.white && track.color.a > 0.2f)
                  {
                      GUI.color = track.color;
                      var colorRect = new Rect(trackRect.xMax + 1, trackRect.yMin, 2, track.finalHeight);
                      GUI.DrawTexture(colorRect, whiteTexture);
                  }

                  GUI.color = Color.white;
                  //

                  //
                  GUI.BeginGroup(trackRect);
                  track.OnTrackInfoGUI(trackRect);
                  GUI.EndGroup();
                  //

                  AddCursorRect(trackRect, pickedTrack == null  || !isPressed ? MouseCursor.Link : MouseCursor.MoveArrow);

                  //CONTEXT
                  if (e.type == EventType.ContextClick && trackRect.Contains(e.mousePosition))
                  {
                      var menu = new GenericMenu();
                      menu.AddItem(new GUIContent("Disable Track"), !track.isActive, () => { track.isActive = !track.isActive; });
                      menu.AddItem(new GUIContent("Lock Track"), track.isLocked, () => { track.isLocked = !track.isLocked; });
                      menu.AddItem(new GUIContent("Copy"), false, () => { copyTrack = track; });
                      if (track.GetType().RTGetAttribute<UniqueElementAttribute>(true) == null)
                      {
                          menu.AddItem(new GUIContent("Duplicate"), false, () =>
                          {
                              actionTimeline.DuplicateTrack(track);
                              InitClipWrappers();
                          });
                      }
                      else
                      {
                          menu.AddDisabledItem(new GUIContent("Duplicate"));
                      }

                      menu.AddSeparator("/");
                      menu.AddItem(new GUIContent("Delete Track"), false, () =>
                      {
                          if (EditorUtility.DisplayDialog("Delete Track", "Are you sure?", "YES", "NO!"))
                          {
                              actionTimeline.DeleteTrack(track);
                              InitClipWrappers();
                          }
                      });
                      menu.ShowAsContext();
                      e.Use();
                  }

                  //REORDERING
                  if (e.type == EventType.MouseDown && e.button == 0 && trackRect.Contains(e.mousePosition))
                  {
                      pickedTrack = track;
                      Selection.activeObject = track;
                      isPressed = true;
                      e.Use();
                  }

                  if (pickedTrack != null )
                  {
                      if (pickedTrack != track)
                      {
                          if (trackRect.Contains(e.mousePosition) && isPressed)
                          {
                              var markRect = new Rect(trackRect.x, (actionTimeline.tracks.IndexOf(pickedTrack) < t) ? trackRect.yMax - 2 : trackRect.y, trackRect.width, 2);
                              GUI.color = Color.grey;
                              GUI.DrawTexture(markRect, Styles.whiteTexture);
                              GUI.color = Color.white;
                          }

                          if (e.rawType == EventType.MouseUp && e.button == 0 && trackRect.Contains(e.mousePosition))
                          {
                              actionTimeline.tracks.Remove(pickedTrack);
                              actionTimeline.tracks.Insert(t, pickedTrack);
                              //actionTimeline.Validate();
                              pickedTrack = null;
                              e.Use();
                          }
                      }

                      if (e.rawType == EventType.MouseUp && e.button == 0 )
                      {
                          isPressed = false;
                      }
                    
                  }
                  
              }
          }

          void ShowTimeLines(Rect centerRect)
          {
              var e = Event.current;

              //bg graphic
              var bgRect = Rect.MinMaxRect(centerRect.xMin, TOP_MARGIN + TOOLBAR_HEIGHT + scrollPos.y, centerRect.xMax,
                  screenHeight - TOOLBAR_HEIGHT + scrollPos.y);
              GUI.color = Color.black.WithAlpha(0.1f);
              GUI.DrawTexture(bgRect, whiteTexture);
              GUI.color = Color.black.WithAlpha(0.03f);
              GUI.DrawTextureWithTexCoords(bgRect, Styles.stripes, new Rect(0, 0, bgRect.width / -7, bgRect.height / -7));
              GUI.color = Color.white;

              // draw guides based on time info stored
              for (var _i = timeInfoStart; _i <= timeInfoEnd; _i += timeInfoInterval)
              {
                  var i = Mathf.Round(_i * 10) / 10;
                  DrawGuideLine(i, Color.black.WithAlpha(0.05f));
                  if (i % timeInfoHighMod == 0)
                  {
                      DrawGuideLine(i, Color.black.WithAlpha(0.05f));
                  }
              }


              //Begin Group
              GUI.BeginGroup(centerRect);

              //starting height
              var nextYPos = FIRST_GROUP_TOP_MARGIN;

              //master sections
              var sectionsRect = Rect.MinMaxRect(Mathf.Max(TimeToPos(viewTimeMin), TimeToPos(0)), 3, TimeToPos(viewTimeMax), 18);


              //Begin Windows
              BeginWindows();

              //GROUPS

              //TRACKS
              if (actionTimeline)
              {
                  for (int t = 0; t < actionTimeline.tracks.Count; t++)
                  {
                      var track = actionTimeline.tracks[t];
                      var yPos = nextYPos;
                      var trackPosRect = Rect.MinMaxRect(Mathf.Max(TimeToPos(viewTimeMin), TimeToPos(track.startTime)), yPos, TimeToPos(viewTimeMax),
                          yPos + track.finalHeight);
                      var trackTimeRect = Rect.MinMaxRect(Mathf.Max(viewTimeMin, track.startTime), 0, viewTimeMax, 0);
                      nextYPos += track.finalHeight + TRACK_MARGINS;

                      //GRAPHICS
                      GUI.color = Color.black.WithAlpha(isProSkin ? 0.06f : 0.1f);
                      GUI.DrawTexture(trackPosRect, whiteTexture);
                      Handles.color = ColorUtility.Grey(isProSkin ? 0.15f : 0.4f);
                      Handles.DrawLine(new Vector2(TimeToPos(viewTimeMin), trackPosRect.y + 1), new Vector2(trackPosRect.xMax, trackPosRect.y + 1));
                      Handles.DrawLine(new Vector2(TimeToPos(viewTimeMin), trackPosRect.yMax), new Vector2(trackPosRect.xMax, trackPosRect.yMax));
                      if (track.showCurves)
                      {
                          Handles.DrawLine(new Vector2(trackPosRect.x, trackPosRect.y + track.defaultHeight),
                              new Vector2(trackPosRect.xMax, trackPosRect.y + track.defaultHeight));
                      }

                      Handles.color = Color.white;
                      if (viewTimeMin < 0)
                      {
                          //just visual clarity
                          GUI.Box(Rect.MinMaxRect(TimeToPos(viewTimeMin), trackPosRect.yMin, TimeToPos(0), trackPosRect.yMax), string.Empty);
                      }

                      if (track.startTime > actionTimeline.startTime || track.endTime < actionTimeline.endTime)
                      {
                          Handles.color = Color.white;
                          GUI.color = Color.black.WithAlpha(0.2f);
                          if (track.startTime > actionTimeline.startTime)
                          {
                              var tStart = TimeToPos(track.startTime);
                              var r = Rect.MinMaxRect(TimeToPos(0), yPos, tStart, yPos + track.finalHeight);
                              GUI.DrawTexture(r, whiteTexture);
                              GUI.DrawTextureWithTexCoords(r, Styles.stripes, new Rect(0, 0, r.width / 7, r.height / 7));
                              var a = new Vector2(tStart, trackPosRect.yMin);
                              var b = new Vector2(a.x, trackPosRect.yMax);
                              Handles.DrawLine(a, b);
                          }

                          if (track.endTime < actionTimeline.endTime)
                          {
                              var tEnd = TimeToPos(track.endTime);
                              var r = Rect.MinMaxRect(tEnd, yPos, TimeToPos(length), yPos + track.finalHeight);
                              GUI.DrawTexture(r, whiteTexture);
                              GUI.DrawTextureWithTexCoords(r, Styles.stripes, new Rect(0, 0, r.width / 7, r.height / 7));
                              var a = new Vector2(tEnd, trackPosRect.yMin);
                              var b = new Vector2(a.x, trackPosRect.yMax);
                              Handles.DrawLine(a, b);
                          }

                          GUI.color = Color.white;
                          Handles.color = Color.white;
                      }

                      GUI.backgroundColor = Color.white;

                      //highlight selected track
                      if (track == pickedTrack && track == Selection.activeObject)
                      {
                          GUI.color = Color.grey;
                          GUI.Box(trackPosRect.ExpandBy(0, 2), string.Empty, Styles.hollowFrameHorizontalStyle);
                          GUI.color = Color.white;
                      }
                      //

                      if (track.isLocked)
                      {
                          if (e.isMouse && trackPosRect.Contains(e.mousePosition))
                          {
                              e.Use();
                          }
                      }

                      //...
                      var cursorTime = SnapTime(PosToTime(mousePosition.x));
                      track.OnTrackTimelineGUI(trackPosRect, trackTimeRect, cursorTime, TimeToPos);
                      //...

                      if (!track.isActive || track.isLocked)
                      {
                          postWindowsGUI += () =>
                          {
                              //overlay dark stripes for disabled tracks
                              if (!track.isActive)
                              {
                                  GUI.color = Color.black.WithAlpha(0.2f);
                                  GUI.DrawTexture(trackPosRect, whiteTexture);
                                  GUI.DrawTextureWithTexCoords(trackPosRect, Styles.stripes,
                                      new Rect(0, 0, (trackPosRect.width / 5), (trackPosRect.height / 5)));
                                  GUI.color = Color.white;
                              }

                              //overlay light stripes for locked tracks
                              if (track.isLocked)
                              {
                                  GUI.color = Color.black.WithAlpha(0.15f);
                                  GUI.DrawTextureWithTexCoords(trackPosRect, Styles.stripes, new Rect(0, 0, trackPosRect.width / 20, trackPosRect.height / 20));
                                  GUI.color = Color.white;
                              }

                              if (isProSkin)
                              {
                                  string overlayLabel = null;
                                  if (!track.isActive && track.isLocked)
                                  {
                                      overlayLabel = "DISABLED & LOCKED";
                                  }
                                  else
                                  {
                                      if (!track.isActive)
                                      {
                                          overlayLabel = "DISABLED";
                                      }

                                      if (track.isLocked)
                                      {
                                          overlayLabel = "LOCKED";
                                      }
                                  }

                                  var size = Styles.centerLabel.CalcSize(new GUIContent(overlayLabel));
                                  var bgLabelRect = new Rect(0, 0, size.x, size.y);
                                  bgLabelRect.center = trackPosRect.center;
                                  GUI.Label(trackPosRect, string.Format("<b>{0}</b>", overlayLabel), Styles.centerLabel);
                                  GUI.color = Color.white;
                              }
                          };
                      }


                      //ACTION CLIPS
                      for (int a = 0; a < track.clips.Count; a++)
                      {
                          var action = track.clips[a];
                          var ID = UID(t, a);
                          ActionClipWrapper clipWrapper = null;

                          clipWrappers ??= new Dictionary<int, ActionClipWrapper>();
                          if (!clipWrappers.TryGetValue(ID, out clipWrapper) || clipWrapper.clip != action)
                          {
                              InitClipWrappers();
                              clipWrapper = clipWrappers[ID];
                          }

                          //find and store next/previous clips to wrapper
                          var nextClip = a < track.clips.Count - 1 ? track.clips[a + 1] : null;
                          var previousClip = a != 0 ? track.clips[a - 1] : null;
                          clipWrapper.nextClip = nextClip;
                          clipWrapper.previousClip = previousClip;


                          //get the action box rect
                          var clipRect = clipWrapper.rect;

                          //modify it
                          clipRect.y = yPos;
                          clipRect.width = Mathf.Max(action.length / viewTime * centerRect.width, 6);
                          clipRect.height = track.defaultHeight;


                          //get the action time and pos
                          var xTime = action.startTime;
                          var xPos = clipRect.x;

                          if (interactingClip != null && ReferenceEquals(interactingClip.clip, action) && interactingClip.isDragging)
                          {
                              var lastTime = xTime;
                              xTime = PosToTime(xPos + leftRect.width);
                              xTime = SnapTime(xTime);
                              xTime = Mathf.Clamp(xTime, 0, maxTime - 0.1f);
                              
                              //Apply xTime
                              action.startTime = xTime;
                          }

                          //apply xPos
                          clipRect.x = TimeToPos(xTime);


                          //dont draw if outside of view range and not selected
                          var isSelected =  ReferenceEquals(Selection.activeObject, action);
                          var isVisible = Rect.MinMaxRect(0, scrollPos.y, centerRect.width, centerRect.height).Overlaps(clipRect);
                          if (!isSelected && !isVisible)
                          {
                              clipWrapper.rect = default(Rect); //we basicaly "nullify" the rect. Too much trouble to work with nullable rect.
                              continue;
                          }

                          //draw selection graphics rect
                          if (isSelected)
                          {
                              var selRect = clipRect.ExpandBy(2);
                              GUI.color = HIGHLIGHT_COLOR;
                              GUI.DrawTexture(selRect, Styles.whiteTexture);
                              GUI.color = Color.white;
                          }

                          //determine color and draw clip
                          var color = Color.white;
                          color = action.isValid ? color : new Color(1, 0.3f, 0.3f);
                          color = track.isActive ? color : Color.grey;
                          GUI.color = color;
                          GUI.Box(clipRect, string.Empty, Styles.clipBoxHorizontalStyle);
                          clipWrapper.rect = GUI.Window(ID, clipRect, ActionClipWindow, string.Empty, GUIStyle.none);
                          if (!isProSkin)
                          {
                              GUI.color = Color.white.WithAlpha(0.5f);
                              GUI.Box(clipRect, string.Empty);
                              GUI.color = Color.white;
                          }

                          GUI.color = Color.white;

                          //forward external Clip GUI
                          var nextPosX = TimeToPos(nextClip != null ? nextClip.startTime : viewTimeMax);
                          var prevPosX = TimeToPos(previousClip != null ? previousClip.endTime : viewTimeMin);
                          var extRectLeft = Rect.MinMaxRect(prevPosX, clipRect.yMin, clipRect.xMin, clipRect.yMax);
                          var extRectRight = Rect.MinMaxRect(clipRect.xMax, clipRect.yMin, nextPosX, clipRect.yMax);
                          action.ShowClipGUIExternal(extRectLeft, extRectRight);

                          //draw info text outside if clip is too small
                          if (clipRect.width <= 20)
                          {
                              GUI.Label(extRectRight, string.Format("<size=9>{0}</size>", action.info));
                          }
                      }
                  }
              }

              EndWindows();

              //call postwindow delegate
              if (postWindowsGUI != null)
              {
                  postWindowsGUI();
                  postWindowsGUI = null;
              }

              //this is done in the same GUI.Group
              //DoMultiSelection();

              GUI.EndGroup();

              //border shadows
              GUI.color = Color.white.WithAlpha(0.2f);
              GUI.Box(bgRect, string.Empty, Styles.shadowBorderStyle);
              GUI.color = Color.white;

              //darken the time after cutscene length
              if (viewTimeMax > length)
              {
                  var endPos = Mathf.Max(TimeToPos(length) + leftRect.width, centerRect.xMin);
                  var darkRect = Rect.MinMaxRect(endPos, centerRect.yMin, centerRect.xMax, centerRect.yMax);
                  GUI.color = Color.black.WithAlpha(0.3f);
                  GUI.Box(darkRect, string.Empty, (GUIStyle)"TextField");
                  GUI.color = Color.white;
              }

              //darken the time before zero
              if (viewTimeMin < 0)
              {
                  var startPos = Mathf.Min(TimeToPos(0) + leftRect.width, centerRect.xMax);
                  var darkRect = Rect.MinMaxRect(centerRect.xMin, centerRect.yMin, startPos, centerRect.yMax);
                  GUI.color = Color.black.WithAlpha(0.3f);
                  GUI.Box(darkRect, string.Empty, (GUIStyle)"TextField");
                  GUI.color = Color.white;
              }

              //ensure no interactive clip
              if (e.rawType == EventType.MouseUp)
              {
                  if (interactingClip != null)
                  {
                      interactingClip.ResetInteraction();
                      interactingClip.EndClipAdjust();
                      interactingClip = null;
                  }
              }
           }
          
          //ActionClip window callback. Its ID is based on the UID function that is based on the index path to the action.
          //The ID of the window is also the same as the ID to use for for clipWrappers dictionary as key to get the clipWrapper for the action that represents this window
          void ActionClipWindow(int id) {
              ActionClipWrapper wrapper = null;
              if ( clipWrappers.TryGetValue(id, out wrapper) ) {
                  wrapper.OnClipGUI(id);
              }
          }
          
        //给Clip的包装器
        class ActionClipWrapper
        {

            const float CLIP_DOPESHEET_HEIGHT = 13f;
            const float SCALE_RECT_WIDTH = 5;

            public BaseActionClip clip;
            public bool isDragging;
            public bool isScalingStart;
            public bool isScalingEnd;
            public bool isControlingBlendIn;
            public bool isControlingBlendOut;

            public Dictionary<AnimationCurve, Keyframe[]> preScaleKeys;
            public float preScaleStartTime;
            public float preScaleEndTime;
            public float preScaleSubclipOffset;
            public float preScaleSubclipSpeed;

            public BaseActionClip previousClip;
            public BaseActionClip nextClip;

            private Event e;
            private int windowID;
            private bool isWaitingMouseDrag;
            private float overlapIn;
            private float overlapOut;
            private float blendInPosX;
            private float blendOutPosX;
            private bool hasActiveParameters;
            private bool hasParameters;
            private float pointerTime;
            private float snapedPointerTime;
            private bool allowScale;

            private Rect dragRect;
            private Rect controlRectIn;
            private Rect controlRectOut;

            private ActionTimelineEditor editor {
                get { return ActionTimelineEditor.current; }
            }
            
            private Rect _rect;
            public Rect rect {
                get { return clip.isCollapsed ? default(Rect) : _rect; }
                set { _rect = value; }
            }

            public ActionClipWrapper(BaseActionClip clip) {
                this.clip = clip;
            }

            public void ResetInteraction() {
                isWaitingMouseDrag = false;
                isDragging = false;
                isControlingBlendIn = false;
                isControlingBlendOut = false;
                isScalingStart = false;
                isScalingEnd = false;
            }

            public void OnClipGUI(int windowID) {
                this.windowID = windowID;
                e = Event.current;

                overlapIn = previousClip != null ? Mathf.Max(previousClip.endTime - clip.startTime, 0) : 0;
                overlapOut = nextClip != null ? Mathf.Max(clip.endTime - nextClip.startTime, 0) : 0;
                blendInPosX = ( clip.blendIn / clip.length ) * rect.width;
                blendOutPosX = ( ( clip.length - clip.blendOut ) / clip.length ) * rect.width;
                hasParameters = clip.hasParameters;
                hasActiveParameters = clip.hasActiveParameters;

                pointerTime = editor.PosToTime(editor.mousePosition.x);
                snapedPointerTime = editor.SnapTime(pointerTime);

                allowScale = clip.CanScale() && clip.length > 0 && rect.width > SCALE_RECT_WIDTH * 2;
                dragRect = new Rect(0, 0, rect.width, rect.height - ( hasActiveParameters ? CLIP_DOPESHEET_HEIGHT : 0 )).ExpandBy(allowScale ? -SCALE_RECT_WIDTH : 0, 0);
                controlRectIn = new Rect(0, 0, SCALE_RECT_WIDTH, rect.height - ( hasActiveParameters ? CLIP_DOPESHEET_HEIGHT : 0 ));
                controlRectOut = new Rect(rect.width - SCALE_RECT_WIDTH, 0, SCALE_RECT_WIDTH, rect.height - ( hasActiveParameters ? CLIP_DOPESHEET_HEIGHT : 0 ));

                editor.AddCursorRect(dragRect, MouseCursor.Link);
                if ( allowScale ) {
                    editor.AddCursorRect(controlRectIn, MouseCursor.ResizeHorizontal);
                    editor.AddCursorRect(controlRectOut, MouseCursor.ResizeHorizontal);
                }

                //...
                var wholeRect = new Rect(0, 0, rect.width, rect.height);
                if ( clip.isLocked && e.isMouse && wholeRect.Contains(e.mousePosition) ) { e.Use(); }
                clip.ShowClipGUI(wholeRect);
                if ( hasActiveParameters && clip.length > 0 ) {
                    ShowClipDopesheet(wholeRect);
                }
                //...


                //set crossblend overlap properties. Do this when no clip is interacting or no clip is dragging
                //this way avoid issue when moving clip on the other side of another, but keep overlap interactive when scaling a clip at least.
                if ( editor.interactingClip == null || !editor.interactingClip.isDragging ) {
                    var overlap = previousClip != null ? Mathf.Max(previousClip.endTime - clip.startTime, 0) : 0;
                    if ( overlap > 0 ) {
                        clip.blendIn = overlap;
                        previousClip.blendOut = overlap;
                    }
                }


                if ( e.type == EventType.MouseDown ) {

                    if ( e.button == 0 ) {
                        if ( dragRect.Contains(e.mousePosition) ) {
                            isWaitingMouseDrag = true;
                        }
                        editor.interactingClip = this;
                        Selection.activeObject = clip;
                        editor.CacheMagnetSnapTimes(clip);
                    }
                }

                if ( e.type == EventType.MouseDrag && isWaitingMouseDrag ) {
                    isDragging = true;
                }

                if ( e.rawType == EventType.ContextClick ) {
                    DoClipContextMenu();
                }


                DrawBlendGraphics();
                DoEdgeControls();


                if ( e.rawType == EventType.MouseUp ) {
                    if ( editor.interactingClip != null ) {
                        editor.interactingClip.EndClipAdjust();
                        editor.interactingClip.ResetInteraction();
                        editor.interactingClip = null;
                    }
                }

                if ( e.button == 0 ) {
                    GUI.DragWindow(dragRect);
                }

                //Draw info text if big enough
                if ( rect.width > 20 ) {
                    var r = new Rect(0, 0, rect.width, rect.height);
                    if ( overlapIn > 0 ) { r.xMin = blendInPosX; }
                    if ( overlapOut > 0 ) { r.xMax = blendOutPosX; }
                    var label = string.Format("<size=10>{0}</size>", clip.info);
                    GUI.color = Color.black;
                    GUI.Label(r, label);
                    GUI.color = Color.white;
                }
            }

            //blend graphics
            void DrawBlendGraphics() {
                if ( clip.blendIn > 0 ) {
                    Handles.color = Color.black.WithAlpha(0.5f);
                    Handles.DrawAAPolyLine(2, new Vector2(0, rect.height), new Vector2(blendInPosX, 0));
                    Handles.color = Color.black.WithAlpha(0.3f);
                    Handles.DrawAAConvexPolygon(new Vector3(0, 0), new Vector3(0, rect.height), new Vector3(blendInPosX, 0));
                }

                if ( clip.blendOut > 0 && overlapOut == 0 ) {
                    Handles.color = Color.black.WithAlpha(0.5f);
                    Handles.DrawAAPolyLine(2, new Vector2(blendOutPosX, 0), new Vector2(rect.width, rect.height));
                    Handles.color = Color.black.WithAlpha(0.3f);
                    Handles.DrawAAConvexPolygon(new Vector3(rect.width, 0), new Vector2(blendOutPosX, 0), new Vector2(rect.width, rect.height));
                }

                if ( overlapIn > 0 ) {
                    Handles.color = Color.black;
                    Handles.DrawAAPolyLine(2, new Vector2(blendInPosX, 0), new Vector2(blendInPosX, rect.height));
                }
                Handles.color = Color.white;
            }

            //clip scale/blend in/out controls
            void DoEdgeControls() {

                var canBlendIn = clip.CanBlendIn() && clip.length > 0;
                var canBlendOut = clip.CanBlendOut() && clip.length > 0;
                if ( !isScalingStart && !isScalingEnd && !isControlingBlendIn && !isControlingBlendOut ) {
                    if ( allowScale || canBlendIn ) {
                        if ( controlRectIn.Contains(e.mousePosition) ) {
                            GUI.BringWindowToFront(windowID);
                            GUI.DrawTexture(controlRectIn.ExpandBy(0, -2), whiteTexture);
                            if ( e.type == EventType.MouseDown && e.button == 0 ) {
                                if ( allowScale && !e.control ) { isScalingStart = true; }
                                if ( canBlendIn && e.control ) { isControlingBlendIn = true; }
                                BeginClipAdjust();
                                e.Use();
                            }
                        }
                    }

                    if ( allowScale || canBlendOut ) {
                        if ( controlRectOut.Contains(e.mousePosition) ) {
                            GUI.BringWindowToFront(windowID);
                            GUI.DrawTexture(controlRectOut.ExpandBy(0, -2), whiteTexture);
                            if ( e.type == EventType.MouseDown && e.button == 0 ) {
                                if ( allowScale && !e.control ) { isScalingEnd = true; }
                                if ( canBlendOut && e.control ) { isControlingBlendOut = true; }
                                BeginClipAdjust();
                                e.Use();
                            }
                        }
                    }
                }

                if ( isControlingBlendIn ) { clip.blendIn = Mathf.Clamp(pointerTime - clip.startTime, 0, clip.length - clip.blendOut); }
                if ( isControlingBlendOut ) { clip.blendOut = Mathf.Clamp(clip.endTime - pointerTime, 0, clip.length - clip.blendIn); }

                if ( isScalingStart ) {
                    var prevTime = previousClip != null ? previousClip.endTime : 0;
                    //magnet snap
                    if ( Prefs.magnetSnapping && !e.control ) {
                        var snapStart = editor.MagnetSnapTime(snapedPointerTime, editor.magnetSnapTimesCache);
                        if ( snapStart != null ) {
                            snapedPointerTime = snapStart.Value;
                            editor.pendingGuides.Add(new GuideLine(snapedPointerTime, Color.red));
                        }
                    }

                    if ( clip.CanCrossBlend(previousClip) ) { prevTime -= Mathf.Min(clip.length / 2, previousClip.length / 2); }

                    clip.startTime = snapedPointerTime;
                    clip.startTime = Mathf.Clamp(clip.startTime, prevTime, preScaleEndTime);
                    clip.endTime = preScaleEndTime;
                    clip.ClipLengthChanged();
                    UpdateClipAdjustContents();
                }

                if ( isScalingEnd ) {
                    var nextTime = nextClip != null ? nextClip.startTime : editor.maxTime;
                    //magnet snap
                    if ( Prefs.magnetSnapping && !e.control ) {
                        var snapEnd = editor.MagnetSnapTime(snapedPointerTime, editor.magnetSnapTimesCache);
                        if ( snapEnd != null ) {
                            snapedPointerTime = snapEnd.Value;
                            editor.pendingGuides.Add(new GuideLine(snapedPointerTime, Color.red));
                        }
                    }

                    if ( clip.CanCrossBlend(nextClip) ) { nextTime += Mathf.Min(clip.length / 2, nextClip.length / 2); }

                    clip.endTime = snapedPointerTime;
                    clip.endTime = Mathf.Clamp(clip.endTime, 0, nextTime);
                    clip.ClipLengthChanged();
                    UpdateClipAdjustContents();
                }
            }


            //store pre adjust values
            public void BeginClipAdjust() {
                preScaleStartTime = clip.startTime;
                preScaleEndTime = clip.endTime;
                preScaleKeys = clip.GetCurvesAll().ToDictionary(k => k, k => k.keys);
                if ( clip is ISubClipContainable ) {
                    preScaleSubclipOffset = ( clip as ISubClipContainable ).subClipOffset;
                    preScaleSubclipSpeed = ( clip as ISubClipContainable ).subClipSpeed;
                }
                editor.CacheMagnetSnapTimes(clip);
            }

            //retime keys lerp between start/end time.
            public void UpdateClipAdjustContents() {

                if ( preScaleKeys == null ) { return; }

                var retime = Event.current.control || Prefs.retimeMode;
                var trim = !Event.current.shift && !Prefs.rippleMode && !retime;

                foreach ( var curve in clip.GetCurvesAll() ) {
                    for ( var i = 0; i < curve.keys.Length; i++ ) {
                        var preKey = preScaleKeys[curve][i];

                        if ( retime ) {
                            var preLength = preScaleEndTime - preScaleStartTime;
                            var newTime = Mathf.LerpUnclamped(0, clip.length, preKey.time / preLength);
                            preKey.time = newTime;
                        }

                        if ( trim ) {
                            preKey.time -= clip.startTime - preScaleStartTime;
                        }

                        curve.MoveKey(i, preKey);
                    }

                    curve.UpdateTangentsFromMode();
                }

                ActionTimelineUtility.RefreshAllAnimationEditorsOf(clip.animationData);

                if ( clip is ISubClipContainable ) {
                    if ( trim ) {
                        var subClip = (ISubClipContainable)clip;
                        var delta = preScaleStartTime - clip.startTime;
                        var newOffset = preScaleSubclipOffset + delta;
                        subClip.subClipOffset = newOffset;
                    }
                }
            }

            //flush pre adjust values
            public void EndClipAdjust() {
                preScaleKeys = null;
                if ( Prefs.autoCleanKeysOffRange ) {
                    CleanKeysOffRange();
                }
            }


            //TODO
            // 分割功能，还没做
            ///<summary>Split the clip in two, at specified local time</summary>
            public BaseActionClip Split(float time) {

                if ( !clip.IsTimeWithinClip(time) ) {
                    return null;
                }

                if ( hasParameters ) {
                    foreach ( var param in clip.animationData.animatedParameters ) {
                        if ( param.HasAnyKey() ) { param.TryKeyIdentity(clip.ToLocalTime(time)); }
                    }
                }

                ActionTimelineUtility.CopyClip(clip);
                var copy = ActionTimelineUtility.PasteClip((BaseActionTrack)clip.parent, time);
                copy.startTime = time;
                copy.endTime = clip.endTime;
                clip.endTime = time;
                copy.blendIn = 0;
                clip.blendOut = 0;
                ActionTimelineUtility.FlushCopy();

                var delta = clip.length;
                if ( hasParameters ) {
                    foreach ( var curve in copy.GetCurvesAll() ) {
                        curve.OffsetCurveTime(-delta);
                        curve.RemoveNegativeKeys();
                    }
                   // ActionTimelineUtility.RefreshAllAnimationEditorsOf(clip.animationData);
                }

                if ( copy is ISubClipContainable ) {
                    ( copy as ISubClipContainable ).subClipOffset -= delta;
                }

                return copy;
            }

            ///<summary>Scale clip to fit previous and next</summary>
            /// TODO:
            ///     部分无效
            public void StretchFit() {
                var wasStartTime = clip.startTime;
                var wasEndTime = clip.endTime;
                var targetStart = previousClip != null ? previousClip.endTime : clip.parent.startTime;
                var targetEnd = nextClip != null ? nextClip.startTime : clip.parent.endTime;
                if ( previousClip == null || previousClip.endTime < clip.startTime ) {
                    clip.startTime = targetStart;
                    clip.endTime = wasEndTime;
                }
                if ( nextClip == null || nextClip.startTime > clip.endTime ) {
                    clip.endTime = targetEnd;
                }

                var delta = wasStartTime - clip.startTime;
                if ( hasParameters ) {
                    foreach ( var curve in clip.GetCurvesAll() ) {
                        curve.OffsetCurveTime(delta);
                    }
                    //CutsceneUtility.RefreshAllAnimationEditorsOf(clip.animationData);
                }

                if ( clip is ISubClipContainable ) {
                    ( clip as ISubClipContainable ).subClipOffset += delta;
                }
            }

            ///<summary>Clean keys off clip range after adding a key at 0 and length if there is any key outside that range</summary>
            public void CleanKeysOffRange() {
                if ( hasParameters ) {
                    foreach ( var param in clip.animationData.animatedParameters ) {
                        if ( param.HasAnyKey() ) {
                            if ( param.GetKeyPrevious(0) < 0 ) {
                                param.TryKeyIdentity(0);
                            }
                            if ( param.GetKeyNext(clip.length) > clip.length ) {
                                param.TryKeyIdentity(clip.length);
                            }
                        }
                    }
                    foreach ( var curve in clip.GetCurvesAll() ) {
                        curve.RemoveKeysOffRange(0, clip.length);
                        curve.UpdateTangentsFromMode();
                    }
                    ActionTimelineUtility.RefreshAllAnimationEditorsOf(clip.animationData);
                }
            }

            //Show the clip dopesheet
            void ShowClipDopesheet(Rect rect) {
                var dopeRect = new Rect(0, rect.height - CLIP_DOPESHEET_HEIGHT, rect.width, CLIP_DOPESHEET_HEIGHT);
                GUI.color = isProSkin ? new Color(0, 0.2f, 0.2f, 0.5f) : new Color(0, 0.8f, 0.8f, 0.5f);
                GUI.Box(dopeRect, string.Empty, Styles.clipBoxHorizontalStyle);
                GUI.color = Color.white;
              //  DopeSheetEditor.DrawDopeSheet(clip.animationData, clip, dopeRect, 0, clip.length, false);
            }

            //CONTEXT
            void DoClipContextMenu() {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Copy Clip"), false, () => { ActionTimelineUtility.CopyClip(clip); });
                menu.AddItem(new GUIContent("Cut Clip"), false, () => { ActionTimelineUtility.CutClip(clip); });

                if ( allowScale ) {
                    menu.AddItem(new GUIContent("Fit Clip (F)"), false, () => { StretchFit(); });
                    if ( clip.length > 0 ) {
                        menu.AddItem(new GUIContent("Split At Cursor"), false, () => { Split(snapedPointerTime); });
                        menu.AddItem(new GUIContent("Split At Scrubber (S)"), false, () => { Split(editor.currentTime); });
                    }
                }

                if ( hasParameters ) {
                    menu.AddItem(new GUIContent("Key At Cursor"), false, () => { clip.TryAddIdentityKey(clip.ToLocalTime(snapedPointerTime)); });
                    menu.AddItem(new GUIContent("Key At Scrubber (K)"), false, () => { clip.TryAddIdentityKey(clip.RootTimeToLocalTime()); });
                }

                menu.AddSeparator("/");

                if ( hasActiveParameters ) {
                    menu.AddItem(new GUIContent("Clean Keys Off-Range (C)"), false, () => { CleanKeysOffRange(); });
                    menu.AddItem(new GUIContent("Remove Animation"), false, () =>
                    {
                        if ( EditorUtility.DisplayDialog("Remove Animation", "All Animation Curve keys of all animated parameters for this clip will be removed.\nAre you sure?", "Yes", "No") ) {
                            editor.SafeDoAction(() => { clip.ResetAnimatedParameters(); });
                        }
                    });
                }

                menu.AddItem(new GUIContent("Delete Clip"), false, () =>
                {
                    editor.SafeDoAction(() =>
                    {
                        ( clip.parent as BaseActionTrack )?.DeleteAction(clip);
                        editor.InitClipWrappers();
                    });
                });

                menu.ShowAsContext();
                e.Use();
            }
        }
    }
}

#endif
