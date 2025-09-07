using System;
using System.Collections.Generic;
using System.Linq;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ilsActionEditor
{
    [Icon(typeof(CircleCollider2D))]
    public class BaseActionTrack : ScriptableObject,IDirectable
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private Color _color = Color.white;
        [SerializeField, HideInInspector]
        private bool _active = true;
        [SerializeField, HideInInspector]
        private bool _isLocked = false;
        [SerializeField]
        private List<BaseActionClip> _actionClips = new List<BaseActionClip>();

        ///<summary>The actor to be used in the track taken from it's parent group</summary>
        public GameObject actor {
            get { return parent != null ? parent.actor : null; }
        }
        
        ///<summary>The name...</summary>
        public new string name {
            get { return string.IsNullOrEmpty(_name) ? GetType().Name.SplitCamelCase() : _name; }
            set
            {
                if ( _name != value ) {
                    _name = value;
                    base.name = value;
                }
            }
        }
        
        ///<summary>Coloring of clips within this track</summary>
        public Color color {
            get { return _color.a > 0.1f ? _color : Color.white; }
        }
        
        ///<summary>All action clips of this track</summary>
        public List<BaseActionClip> clips {
            get { return _actionClips; }
            set { _actionClips = value; }
        }
        
        ///<summary>Display info</summary>
        public virtual string info {
            get { return string.Empty; }
        }
        
        ///<summary>Children</summary>
        IEnumerable<IDirectable> IDirectable.children {
            get { return clips.Cast<IDirectable>(); }
        }
        
        ///<summary>
        /// Type-based order of track
        /// 不太懂是干什么的
        /// </summary>
        public int layerOrder { get; private set; }
        
        ///<summary>Root director</summary>
        public IDirector root { get { return parent != null ? parent.root : null; } }
        ///<summary>Parent directable</summary>
        [SerializeField,ShowInInspector]
        private ActionTimeline _parent;
        
        public IDirectable parent
        {
            get => _parent;
        }

        private bool _isCollapsed = false;
        public virtual bool isCollapsed {
            get { return _isCollapsed; }
            set { _isCollapsed = value; }
        }
        ///<summary>Is active and used?</summary>
        [ShowInInspector]
        public virtual  bool isActive {
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
        ///<summary>Editor is locked?</summary>
        public virtual bool isLocked {
            get=> _isLocked;
            set { _isLocked = value; }
        }
        ///<summary>Start time, usually parent.startTime</summary>
        [ShowInInspector]
        public virtual float startTime {
            get { return parent != null ? parent.startTime : 0; }
            set { }
        }

        public virtual int startFrame
        {
            get => Mathf.FloorToInt(startTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        }
        
        ///<summary>Start time, usually parent.endTime</summary>
        [ShowInInspector]
        public virtual float endTime {
            get { return parent != null ? parent.endTime : 0; }
            set { }
        }

        public virtual int endFrame
        {
            get => Mathf.FloorToInt(endTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        }
        
        ///<summary>Blend in</summary>
        public virtual float blendIn {
            get { return 0f; }
            set { }
        }
        ///<summary>Blend out</summary>
        virtual public float blendOut {
            get { return 0f; }
            set { }
        }
        ///<summary>Able to cross-blend?</summary>
        public bool canCrossBlend {
            get { return false; }
        }
        
        //when the cutscene init (once, awake)
        bool IDirectable.Initialize() { return OnInitialize(); }
        virtual protected bool OnInitialize() { return true; }
        //when the cutscene starts
        void IDirectable.Enter() { OnEnter(); }
        virtual protected void OnEnter() { }
        //when the cutscene is updated
        void IDirectable.Update() { OnUpdate(); }
        virtual protected void OnUpdate() { }
        
        void IDirectable.LogicUpdate() { OnLogicUpdate(); }
        
        virtual protected void OnLogicUpdate() { }
        //when the cutscene stops
        void IDirectable.Exit() { OnExit(); }
        virtual protected void OnExit() { }
        //when the cutscene enters backwards
        void IDirectable.ReverseEnter() { OnReverseEnter(); }
        virtual protected void OnReverseEnter() { }
        //when the cutscene is reversed/rewinded
        void IDirectable.Reverse() { OnReverse(); }
        virtual protected void OnReverse() { }
        //when root is enabled/started
        void IDirectable.RootEnabled() { OnRootEnabled(); }
        virtual protected void OnRootEnabled() { }
        //when root is disabled/finished
        void IDirectable.RootDisabled() { OnRootDisabled(); }
        virtual protected void OnRootDisabled() { }
        //when root is updated
        void IDirectable.RootUpdated(float time, float previousTime) { OnRootUpdated(time, previousTime); }
        virtual protected void OnRootUpdated(float time, float previousTime) { }
        //when root is destroyed
        void IDirectable.RootDestroyed() { OnRootDestroyed(); }
        virtual protected void OnRootDestroyed() { }
        
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
            OnLogicUpdate(time, previousTime, logicFrameCount);
        }
        public virtual void OnLogicUpdate(float time, float previousTime,int logicFrameCount) { }

        void IDirectable.LogicExit()
        {
            OnLogicExit();
        }
        public virtual void OnLogicExit() { }*/
        
        
#if UNITY_EDITOR
        //Gizmos selected
        void IDirectable.DrawGizmos(bool selected) { if ( selected ) OnDrawGizmosSelected(); }
        virtual protected void OnDrawGizmosSelected() { }

        //Scene GUI stuff
        void IDirectable.SceneGUI(bool selected) { OnSceneGUI(); }
        virtual protected void OnSceneGUI() { }
#endif
        //TODO
        //一堆需要修改的
        ///<summary>After creation</summary>
        public void PostCreate(ActionTimeline parent) {
            _parent = parent;
            OnCreate();
        }
        virtual protected void OnCreate() { }
        ///<summary>Validate the track and it's clips</summary>
        public void Validate(IDirector root, IDirectable parent) {
           // this.parent = parent;
           //clips = GetComponents<BaseActionClip>().OrderBy(a => a.startTime).ToList();
          //  layerOrder = parent.children.Where(t => t.GetType() == this.GetType() && t.isActive).Reverse().ToList().IndexOf(this); // O.o
            OnAfterValidate();
        }
        virtual protected void OnAfterValidate() { }
        ///----------------------------------------------------------------------------------------------

        ///----------------------------------------------------------------------------------------------
        /// 
        public float GetTrackWeight() { return this.GetWeight(root.currentTime - this.startTime, this.blendIn, this.blendOut); }
        public float GetTrackWeight(float time) { return this.GetWeight(time, this.blendIn, this.blendOut); }
        public float GetTrackWeight(float time, float blendInOut) { return this.GetWeight(time, blendInOut, blendInOut); }
        public float GetTrackWeight(float time, float blendIn, float blendOut) { return this.GetWeight(time, blendIn, blendOut); }
        ///----------------------------------------------------------------------------------------------

        ///----------------------------------------------------------------------------------------------
        
#if !UNITY_EDITOR //runtime add/delete action
        ///<summary>Add an ActionClip to this Track</summary>
        public T AddAction<T>(float time) where T : ActionClip { return (T)AddAction(typeof(T), time); }
        public ActionClip AddAction(System.Type type, float time) {
            var newAction = gameObject.AddComponent(type) as ActionClip;
            newAction.startTime = time;
            clips.Add(newAction);
            newAction.PostCreate(this);

            var nextAction = clips.FirstOrDefault(a => a.startTime > newAction.startTime);
            if ( nextAction != null ) { newAction.endTime = Mathf.Min(newAction.endTime, nextAction.startTime); }

            root.Validate();
            return newAction;
        }

        ///<summary>Remove an ActionClip from this Track</summary>
        public void DeleteAction(ActionClip action) {
            clips.Remove(action);
            DestroyImmediate(action);
            root.Validate();
        }
#endif
        ///----------------------------------------------------------------------------------------------


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
        
#if UNITY_EDITOR
        private const float PARAMS_TOP_MARGIN = 5f;
        private const float PARAMS_LINE_HEIGHT = 18f;
        private const float PARAMS_LINE_MARGIN = 2f;
        private const float BOX_WIDTH = 30f;
        
        [SerializeField, HideInInspector]
        private float _customHeight = 300f;

        private bool isResizingHeight = false;
        private float proposedHeight = 0f;
        private int inspectedParameterIndex = -1;
        private object _icon;
        private bool _showAnimationCurves;
        private BaseActionClip _showCurvesClip;

        [UnityEditor.InitializeOnLoadMethod]
        static void Editor_Init() {
            ActionTimelineUtility.onSelectionChange += OnDirectableSelectionChange;
        }
        //basically store last clip selected so that we can show multiple clip curves from different tracks at the same time
        //基本上存储最后选择的剪辑，以便我们可以同时显示来自不同轨迹的多个剪辑曲线
        static void OnDirectableSelectionChange(IDirectable directable) {
            if ( directable is IKeyable && directable.parent is BaseActionTrack ) {
                ( directable.parent as BaseActionTrack).showCurvesClip = (IKeyable)directable;
            }
        }
        
        ///<summary>The expansion height</summary>
        private float customHeight {
            get { return _customHeight; }
            set { _customHeight = Mathf.Clamp(value, proposedHeight, 500); }
        }
        
        ///<summary>The final shown height</summary>
        public float finalHeight {
            get
            {
                if ( showCurves ) {
                    return inspectedParameterIndex == -1 ? Mathf.Max(proposedHeight, defaultHeight + 50) : Mathf.Max(proposedHeight, customHeight);
                }
                return defaultHeight;
            }
        }
        //are curves shown?
        public virtual  bool showCurves {
            get { return _showAnimationCurves; }
            set { _showAnimationCurves = value; }
        }
        
        private IKeyable showCurvesClip {
            get
            {
                if ( _showCurvesClip == null || Equals(_showCurvesClip, null) ) {
                    return null;
                }
                return _showCurvesClip;
            }
            set { _showCurvesClip = (BaseActionClip)value; }
        }
        
        ///<summary>The default track height when not expanded</summary>
        public virtual  float  defaultHeight {
            get { return 32f; }
        }
        
        ///<summary>Icon shown on left if any</summary>
        public virtual Texture icon {
            get
            {
                if ( _icon == null ) {
                    var att = this.GetType().RTGetAttribute<IconAttribute>(true);
                    if ( att != null ) {
                        _icon = Resources.Load(att.iconName) as Texture;
                        if ( _icon == null && !string.IsNullOrEmpty(att.iconName) ) {
                            _icon = UnityEditor.EditorGUIUtility.FindTexture(att.iconName) as Texture;
                        }
                        if ( _icon == null && att.fromType != null ) {
                            _icon = UnityEditor.AssetPreview.GetMiniTypeThumbnail(att.fromType);
                        }
                    }

                    if ( _icon == null ) {
                        _icon = new object();
                    }
                }

                return _icon as Texture;
            }
        }
        ///----------------------------------------------------------------------------------------------

        ///<summary>Add an BaseActionClip to this Track</summary>
        public T AddAction<T>(float time) where T : BaseActionClip { return (T)AddAction(typeof(T), time); }
        
        
        /// <summary>
        /// TODO
        ///  需要修改的函数，把增加组件变成生成新的ScriptedObject
        /// </summary>
        /// <param name="type"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public BaseActionClip AddAction(System.Type type, float time) {

            var catAtt = type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
            if ( catAtt != null && clips.Count == 0 ) {
                name = catAtt.category + " Track";
            }
            
            var newAction = (BaseActionClip)ScriptableObject.CreateInstance(type);
            if (newAction)
            {
                newAction.startTime = time;

                AssetDatabase.AddObjectToAsset(newAction,this );
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                clips.Add(newAction);
                
                
                newAction.PostCreate(this);
                var nextAction = clips.FirstOrDefault(a => a.startTime > newAction.startTime);
                if ( nextAction != null ) {
                    newAction.endTime = Mathf.Min(newAction.endTime, nextAction.startTime);
                }
                Selection.activeObject = newAction;
                return newAction;
            }
            root.Validate();
           return null;
        }
        
        
        ///<summary>
        /// TODO
        ///  需要修改 ，CutsceneUtility相关
        /// Remove an ActionClip from this Track
        /// </summary>
        public void DeleteAction(BaseActionClip action) {
            clips.Remove(action);
            AssetDatabase.RemoveObjectFromAsset(action);
            Object.DestroyImmediate(action,true);
            AssetDatabase.SaveAssets();
            root?.Validate();
        }
        
        ///----------------------------------------------------------------------------------------------
        
        ///<summary>The Editor GUI in the track info on the left</summary>
        public virtual void OnTrackInfoGUI(Rect trackRect) {
            var e = Event.current;
            DoDefaultInfoGUI(e, trackRect);
            if ( showCurves ) {
                var wasEnable = GUI.enabled;
                GUI.enabled = true;
                //TODO
               // DoParamsInfoGUI(e, trackRect, showCurvesClip, showCurvesClip is ActionClips.AnimateProperties);
                GUI.enabled = wasEnable;
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }
        
        //default track info gui 
        protected void DoDefaultInfoGUI(Event e, Rect trackRect) {

            var iconBGRect = new Rect(0, 0, BOX_WIDTH, defaultHeight);
            iconBGRect = iconBGRect.ExpandBy(-1);
            var textInfoRect = Rect.MinMaxRect(iconBGRect.xMax + 2, 0, trackRect.width - BOX_WIDTH - 2, defaultHeight);
            var curveButtonRect = new Rect(trackRect.width - BOX_WIDTH, 0, BOX_WIDTH, defaultHeight);


            GUI.color = Color.black.WithAlpha(UnityEditor.EditorGUIUtility.isProSkin ? 0.1f : 0.1f);
            GUI.DrawTexture(iconBGRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if ( icon != null ) {
                var iconRect = new Rect(0, 0, 16, 16);
                iconRect.center = iconBGRect.center;
                GUI.color = ReferenceEquals(ActionTimelineUtility.selectedObject, this) ? Color.white : new Color(1, 1, 1, 0.8f);
                GUI.DrawTexture(iconRect, icon);
                GUI.color = Color.white;
            }


            var nameString = string.Format("<size=11>{0}</size>", name);
            var infoString = string.Format("<size=9><color=#707070>{0}</color></size>", info);
            GUI.color = isActive ? Color.white : Color.grey;
            GUI.Label(textInfoRect, string.Format("{0}\n{1}", nameString, infoString));
            GUI.color = Color.white;

            var wasEnable = GUI.enabled;
            GUI.enabled = true;
            var curveIconRect = new Rect(0, 0, 16, 16);
            curveIconRect.center = curveButtonRect.center - new Vector2(0, 1);
            var curveIconColor = UnityEditor.EditorGUIUtility.isProSkin ? Color.white : Color.black;
            curveIconColor.a = showCurves ? 1 : 0.3f;

            if ( GUI.Button(curveButtonRect, string.Empty, GUIStyle.none) ) {
                showCurves = !showCurves;
            }

            curveButtonRect = curveButtonRect.ExpandBy(-4);
            GUI.color = ColorUtility.Grey(UnityEditor.EditorGUIUtility.isProSkin ? 0.2f : 1f).WithAlpha(0.2f);
            GUI.Box(curveButtonRect, string.Empty, Styles.clipBoxStyle);

            GUI.color = curveIconColor;
            GUI.DrawTexture(curveIconRect, Styles.curveIcon);

            GUI.color = UnityEditor.EditorGUIUtility.isProSkin ? Color.grey : Color.grey;
            if ( !isActive ) {
                var hiddenRect = new Rect(0, 0, 16, 16);
                hiddenRect.center = curveButtonRect.center - new Vector2(curveButtonRect.width, 0);
                if ( GUI.Button(hiddenRect, Styles.hiddenIcon, GUIStyle.none) ) { isActive = !isActive; }
            }

            if ( isLocked ) {
                var lockRect = new Rect(0, 0, 16, 16);
                lockRect.center = curveButtonRect.center - new Vector2(curveButtonRect.width, 0);
                if ( !isActive ) { lockRect.center -= new Vector2(16, 0); }
                if ( GUI.Button(lockRect, Styles.lockIcon, GUIStyle.none) ) { isLocked = !isLocked; }
            }


            GUI.color = Color.white;
            GUI.enabled = wasEnable;
        }
        
           //show selected clip animated parameters list info
        protected void DoParamsInfoGUI(Event e, Rect trackRect, IKeyable keyable, bool showAddPropertyButton) {

            //bg graphic
            var expansionRect = Rect.MinMaxRect(5, defaultHeight, trackRect.width - 3, finalHeight - 3);
            GUI.color = UnityEditor.EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.7f, 0.7f, 0.7f);
            GUI.DrawTexture(expansionRect, Styles.whiteTexture);
            GUI.color = new Color(0, 0, 0, 0.05f);
            GUI.Box(expansionRect, string.Empty, Styles.shadowBorderStyle);
            GUI.color = Color.white;


            //allow resize height
            if ( inspectedParameterIndex >= 0 ) {
                var resizeRect = Rect.MinMaxRect(0, finalHeight - 4, trackRect.width, finalHeight);
                UnityEditor.EditorGUIUtility.AddCursorRect(resizeRect, UnityEditor.MouseCursor.ResizeVertical);
                GUI.color = Color.grey;
                GUI.DrawTexture(resizeRect, Styles.whiteTexture);
                GUI.color = Color.white;
                if ( e.type == EventType.MouseDown && e.button == 0 && resizeRect.Contains(e.mousePosition) ) { isResizingHeight = true; e.Use(); }
                if ( e.type == EventType.MouseDrag && isResizingHeight ) { customHeight += e.delta.y; }
                if ( e.rawType == EventType.MouseUp ) { isResizingHeight = false; }
            }

            proposedHeight = 0f;

            if ( ( ( keyable == null ) || !ReferenceEquals(keyable.parent, this) ) && !ReferenceEquals(keyable, this) ) {
                GUI.Label(expansionRect, "No Clip Selected", Styles.centerLabel);
                inspectedParameterIndex = -1;
                return;
            }

            if ( !showAddPropertyButton ) {
                if ( keyable is BaseActionClip && !( keyable as BaseActionClip ).isValid ) {
                    GUI.Label(expansionRect, "Clip Is Invalid", Styles.centerLabel);
                    return;
                }

                if ( keyable.animationData == null || !keyable.animationData.isValid ) {
                    if ( keyable is BaseActionClip ) {
                        GUI.Label(expansionRect, "Clip Has No Animatable Parameters", Styles.centerLabel);
                        return;
                    }
                }
            }

            proposedHeight = defaultHeight + PARAMS_TOP_MARGIN;
            if ( keyable.animationData != null && keyable.animationData.animatedParameters != null ) {
                if ( inspectedParameterIndex >= keyable.animationData.animatedParameters.Count ) {
                    inspectedParameterIndex = -1;
                }

                var paramsCount = keyable.animationData.animatedParameters.Count;
                for ( var i = 0; i < paramsCount; i++ ) {
                    var animParam = keyable.animationData.animatedParameters[i];
                    var paramRect = new Rect(expansionRect.xMin + 4, proposedHeight, expansionRect.width - 8, PARAMS_LINE_HEIGHT);
                    proposedHeight += PARAMS_LINE_HEIGHT + PARAMS_LINE_MARGIN;

                    GUI.color = inspectedParameterIndex == i ? new Color(0.5f, 0.5f, 1f, 0.4f) : new Color(0, 0.5f, 0.5f, 0.5f);
                    GUI.Box(paramRect, string.Empty, Styles.headerBoxStyle);
                    GUI.color = Color.white;

                    var paramName = string.Format(" <size=10><color=#252525>{0}</color></size>", animParam.ToString());
                    paramName = inspectedParameterIndex == i ? string.Format("<b>{0}</b>", paramName) : paramName;
                    GUI.Label(paramRect, paramName, Styles.leftLabel);

                    var gearRect = new Rect(paramRect.xMax - 16 - 4, paramRect.y, 16, 16);
                    gearRect.center = new Vector2(gearRect.center.x, paramRect.y + ( paramRect.height / 2 ) - 1);
                    GUI.enabled = true;
                    GUI.color = Color.white.WithAlpha(animParam.enabled ? 1 : 0.5f);
                    if ( GUI.Button(gearRect, Styles.gearIcon, GUIStyle.none) ) {
                        //TODO
                        //AnimatableParameterEditor.DoParamGearContextMenu(animParam, keyable);
                    }
                    GUI.enabled = animParam.enabled;
                    if ( GUI.Button(paramRect, string.Empty, GUIStyle.none) ) {
                        inspectedParameterIndex = inspectedParameterIndex == i ? -1 : i;
                        //TODO
                        //CurveEditor.FrameAllCurvesOf(animParam);
                    }
                    GUI.color = Color.white;
                    GUI.enabled = true;
                }

                proposedHeight += PARAMS_TOP_MARGIN;

                if ( inspectedParameterIndex >= 0 ) {
                    var controlRect = Rect.MinMaxRect(expansionRect.x + 6, proposedHeight + 5, expansionRect.xMax - 6, proposedHeight + 50);
                    var animParam = keyable.animationData.animatedParameters[inspectedParameterIndex];
                    GUILayout.BeginArea(controlRect);
                    //TODO
                    //AnimatableParameterEditor.ShowMiniParameterKeyControls(animParam, keyable);
                    GUILayout.EndArea();
                    proposedHeight = controlRect.yMax + 10;
                }
            }

            if ( showAddPropertyButton && inspectedParameterIndex == -1 ) {
                var buttonRect = Rect.MinMaxRect(expansionRect.x + 6, proposedHeight + 5, expansionRect.xMax - 6, proposedHeight + 25);
                var go = keyable.animatedParametersTarget as GameObject;
                GUI.enabled = go != null && root.currentTime <= 0;
                if ( GUI.Button(buttonRect, "Add Property") ) {
                    EditorTools.ShowAnimatedPropertySelectionMenu(go, keyable.TryAddParameter);
                }
                GUI.enabled = true;
                proposedHeight = buttonRect.yMax + 10;
            }

            //consume event
            if ( e.type == EventType.MouseDown && expansionRect.Contains(e.mousePosition) ) {
                e.Use();
            }
        }
        
        ///<summary>The Editor GUI within the timeline rectangle</summary>
        virtual public void OnTrackTimelineGUI(Rect posRect, Rect timeRect, float cursorTime, System.Func<float, float> TimeToPos) {
            var e = Event.current;

            var clipsPosRect = Rect.MinMaxRect(posRect.xMin, posRect.yMin, posRect.xMax, posRect.yMin + defaultHeight);
            DoTrackContextMenu(e, clipsPosRect, cursorTime);

            if ( showCurves ) {
                var curvesPosRect = Rect.MinMaxRect(posRect.xMin, clipsPosRect.yMax, posRect.xMax, posRect.yMax);
                DoClipCurves(e, curvesPosRect, timeRect, TimeToPos, showCurvesClip);
            }
        }
           void DoTrackContextMenu(Event e, Rect clipsPosRect, float cursorTime) {
            if ( e.type == EventType.ContextClick && clipsPosRect.Contains(e.mousePosition) ) {

                var attachableTypeInfos = new List<EditorTools.TypeMetaInfo>();

                var targetClip = GetType().RTGetAttribute<TargetClipAttribute>(false);

                if (targetClip is null)
                {
                    Debug.LogError($"轨道Track:{GetType().Name}未定义目标Clip");
                    return;
                }
                
                var menu = new UnityEditor.GenericMenu();

                foreach (var type in targetClip.clipType)
                {
                    menu.AddItem(new GUIContent($"添加新Clip/{type.Name}"), false, () => { AddAction(type, cursorTime); });
                }
                
                //黏贴功能
                var copyType = ActionTimelineUtility.GetCopyType();
                if ( copyType != null && targetClip.clipType.Contains(copyType)) {
                    menu.AddItem(new GUIContent(string.Format("黏贴Clip", copyType.Name)), false, () => { ActionTimelineUtility.PasteClip(this, cursorTime); });
                }
                menu.ShowAsContext();
                e.Use();

            }
        }

        protected void DoClipCurves(Event e, Rect posRect, Rect timeRect, System.Func<float, float> TimeToPos, IKeyable keyable) {

            //track expanded bg
            GUI.color = Color.black.WithAlpha(0.1f);
            GUI.Box(posRect, string.Empty, Styles.timeBoxStyle);
            GUI.color = Color.white;

            if ( ( ( keyable == null ) || !ReferenceEquals(keyable.parent, this) ) && !ReferenceEquals(keyable, this) ) {
                GUI.color = Color.white.WithAlpha(0.3f);
                GUI.Label(posRect, "Select a Clip of this Track to view it's Animated Parameters here", Styles.centerLabel);
                GUI.color = Color.white;
                return;
            }

            var finalPosRect = posRect;
            var finalTimeRect = timeRect;

            //adjust rects
            if ( keyable is BaseActionClip ) {
                finalPosRect.xMin = Mathf.Max(posRect.xMin, TimeToPos(keyable.startTime));
                finalPosRect.xMax = Mathf.Min(posRect.xMax, TimeToPos(keyable.endTime));
                finalTimeRect.xMin = Mathf.Max(timeRect.xMin, keyable.startTime) - keyable.startTime;
                finalTimeRect.xMax = Mathf.Min(timeRect.xMax, keyable.endTime) - keyable.startTime;
            }

            //add some top/bottom margins
            finalPosRect.yMin += 1;
            finalPosRect.yMax -= 3;
            finalPosRect.width = Mathf.Max(finalPosRect.width, 5);

            //dark bg
            GUI.color = Color.black.WithAlpha(0.4f);
            GUI.DrawTexture(posRect, Styles.whiteTexture);
            GUI.color = Color.white;


            //out of view range
            if ( keyable is BaseActionClip ) {
                if ( keyable.startTime > timeRect.xMax || keyable.endTime < timeRect.xMin ) {
                    return;
                }
            }


            //keyable bg
            GUI.color = UnityEditor.EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f, 0.9f) : new Color(0.7f, 0.7f, 0.7f, 0.9f);
            GUI.Box(finalPosRect, string.Empty, Styles.clipBoxFooterStyle);
            GUI.color = Color.white;

            //if too small do nothing more
            if ( finalPosRect.width <= 5 ) {
                return;
            }

            if ( keyable is BaseActionClip && !( keyable as BaseActionClip ).isValid ) {
                GUI.Label(finalPosRect, "Clip Is Invalid", Styles.centerLabel);
                return;
            }

            if ( keyable.animationData == null || !keyable.animationData.isValid ) {
                if ( keyable is BaseActionClip ) {
                    GUI.Label(finalPosRect, "Clip has no Animatable Parameters", Styles.centerLabel);
                } else {
                    GUI.Label(finalPosRect, "Track has no Animated Properties. You can add some on the left side", Styles.centerLabel);
                }
                return;
            }

            if ( inspectedParameterIndex >= keyable.animationData.animatedParameters.Count ) {
                inspectedParameterIndex = -1;
            }


            //vertical guides from params to dopesheet
            if ( inspectedParameterIndex == -1 ) {
                var yPos = PARAMS_TOP_MARGIN;
                for ( var i = 0; i < keyable.animationData.animatedParameters.Count; i++ ) {
                    // var animParam = keyable.animationData.animatedParameters[i];
                    var paramRect = new Rect(0, posRect.yMin + yPos, finalPosRect.xMin - 2, PARAMS_LINE_HEIGHT);
                    yPos += PARAMS_LINE_HEIGHT + PARAMS_LINE_MARGIN;
                    paramRect.yMin += 1f;
                    paramRect.yMax -= 1f;
                    GUI.color = new Color(0, 0.5f, 0.5f, 0.1f);
                    GUI.DrawTexture(paramRect, Styles.whiteTexture);
                    GUI.color = Color.white;
                }
            }


            //begin in group and neutralize rect
            GUI.BeginGroup(finalPosRect);
            finalPosRect = new Rect(0, 0, finalPosRect.width, finalPosRect.height);

            if ( inspectedParameterIndex == -1 ) {
                var yPos = PARAMS_TOP_MARGIN;
                for ( var i = 0; i < keyable.animationData.animatedParameters.Count; i++ ) {
                    var animParam = keyable.animationData.animatedParameters[i];
                    var paramRect = new Rect(finalPosRect.xMin, finalPosRect.yMin + yPos, finalPosRect.width, PARAMS_LINE_HEIGHT);
                    yPos += PARAMS_LINE_HEIGHT + PARAMS_LINE_MARGIN;
                    paramRect.yMin += 1f;
                    paramRect.yMax -= 1f;
                    GUI.color = Color.black.WithAlpha(0.05f);
                    GUI.DrawTexture(paramRect, Texture2D.whiteTexture);
                    UnityEditor.Handles.color = Color.black.WithAlpha(0.2f);
                    UnityEditor.Handles.DrawLine(new Vector2(paramRect.xMin, paramRect.yMin), new Vector2(paramRect.xMax, paramRect.yMin));
                    UnityEditor.Handles.DrawLine(new Vector2(paramRect.xMin, paramRect.yMax), new Vector2(paramRect.xMax, paramRect.yMax));
                    UnityEditor.Handles.color = Color.white;
                    GUI.color = Color.white;

                    if ( animParam.enabled ) {
                        //TODO
                        //DopeSheetEditor.DrawDopeSheet(animParam, keyable, paramRect, finalTimeRect.x, finalTimeRect.width, true);
                    } else {
                        GUI.color = new Color(0, 0, 0, 0.2f);
                        GUI.DrawTextureWithTexCoords(paramRect, Styles.stripes, new Rect(0, 0, paramRect.width / 7, paramRect.height / 7));
                        GUI.color = Color.white;
                    }
                }
            }

            if ( inspectedParameterIndex >= 0 ) {
                var animParam = keyable.animationData.animatedParameters[inspectedParameterIndex];
                var dopeRect = finalPosRect;
                dopeRect.y += 4f;
                dopeRect.height = 16f;
                //TODO
                //DopeSheetEditor.DrawDopeSheet(animParam, keyable, dopeRect, finalTimeRect.x, finalTimeRect.width, true);
                var curveRect = finalPosRect;
                curveRect.yMin = dopeRect.yMax + 4;
                UnityEditor.Handles.color = Color.black.WithAlpha(0.5f);
                UnityEditor.Handles.DrawLine(new Vector2(curveRect.xMin, curveRect.yMin), new Vector2(curveRect.xMax, curveRect.yMin));
                UnityEditor.Handles.color = Color.white;
                //TODO
                //CurveEditor.DrawCurves(animParam, keyable, curveRect, finalTimeRect);
            }

            //consume event
            if ( e.type == EventType.MouseDown && finalPosRect.Contains(e.mousePosition) ) {
                e.Use();
            }

            GUI.EndGroup();

            /*
                        //darken out of clip range time
                        //will use if I make curve editing full-width
                        if (Prefs.fullWidthCurveEditing){
                            var darkBefore = Rect.MinMaxRect( posRect.xMin, posRect.yMin, Mathf.Max(posRect.xMin, TimeToPos(keyable.startTime)), posRect.yMax );
                            var darkAfter = Rect.MinMaxRect( Mathf.Min(posRect.xMax, TimeToPos(keyable.endTime)), posRect.yMin, posRect.xMax, posRect.yMax );
                            GUI.color = new Color(0.1f,0.1f,0.1f,0.6f);
                            GUI.DrawTexture(darkBefore, Styles.whiteTexture);
                            GUI.DrawTexture(darkAfter, Styles.whiteTexture);
                            GUI.color = Color.white;
                        }
            */

        }
#endif

    }
}