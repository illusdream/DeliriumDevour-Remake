using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ilsActionEditor;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ilsActionEditor
{
    /// <summary>
    /// 所有clip的基类
    /// </summary>
    [Attachable(typeof (BaseActionTrack))]
    public class BaseActionClip : ScriptableObject,IDirectable,IKeyable
    {
        [SerializeField,HideInInspector]
        private float _startTime;
        [SerializeField, HideInInspector]
        private AnimationDataCollection _animationData;
        
        ///<summary>存储所有的动画对象</summary>
        public AnimationDataCollection animationData {
            get { return _animationData; }
            private set { _animationData = value; }
        }
        public IDirector root { get { return parent != null ? parent.root : null; } }
        
        public ActionDirector Director => root as ActionDirector;
        
        public MonoBlackBoard BlackBoard => Director.BlackBoard;
        
        [SerializeField]
        [HideInInspector]
        private BaseActionTrack _parent;

        public IDirectable parent
        {
            get => _parent;
        }

        public IEnumerable<IDirectable> children { get; }
        public GameObject actor { get { return parent != null ? parent.actor : null; } }
        public bool isActive {
            get { return parent != null ? parent.isActive && isValid : false; }
        }
        public bool isCollapsed {
            get { return parent != null ? parent.isCollapsed : false; }
        }
        public bool isLocked {
            get { return parent != null ? parent.isLocked : false; }
        }
        public float startTime {
            get { return _startTime; }
            set
            {
                if ( _startTime != value ) {
                    _startTime = Mathf.Max(value, 0);
                    blendIn = Mathf.Clamp(blendIn, 0, length - blendOut);
                    blendOut = Mathf.Clamp(blendOut, 0, length - blendIn);
                }
            }
        }

        public int startFrame => Mathf.FloorToInt(startTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        
        public float endTime {
            get { return startTime + length; }
            set
            {
                if ( startTime + length != value ) {
                    length = Mathf.Max(value - startTime, 0);
                    blendOut = Mathf.Clamp(blendOut, 0, length - blendIn);
                    blendIn = Mathf.Clamp(blendIn, 0, length - blendOut);
                }
            }
        }

        public int endFrame => Mathf.FloorToInt(endTime * Config.GetFrameworkConfig().LogicUpdateCountPerScecond);
        
        /// <summary>
        /// clip的长度,重写改变此属性：
        /// 0 -》 触发器之类的
        /// 固定长度 -》 固定输出时间。
        /// 变量 -》 可变长度clip
        /// </summary>
        virtual public float length {
            get { return 0; }
            set { }
        }
        
        /// <summary>
        /// clip的融合值，0为不进行混合
        /// </summary>
        virtual public float blendIn {
            get { return 0; }
            set { }
        }
        /// <summary>
        /// clip的融合值，0为不进行混合
        /// </summary>
        virtual public float blendOut {
            get { return 0; }
            set { }
        }
        /// <summary>
        /// 该clip是否可以在同一类型的其他clip之间交叉混合？
        /// </summary>
        virtual public bool canCrossBlend {
            get { return false; }
        }
        
        ///<summary>简短的总结。覆盖此内容以在编辑器中的clip中显示特定内容。</summary>
        virtual public string info {
            get
            {
                var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(true);
                if ( nameAtt != null ) {
                    return nameAtt.name;
                }
                return this.GetType().Name.SplitCamelCase();
            }
        }
        
        ///<summary>这个clip是否正常-》可以正常执行</summary>
        virtual public bool isValid {
            get { return true; }
        }
        
        virtual public TransformSpace defaultTransformSpace {
            get { return TransformSpace.CutsceneSpace; }
        }
        
        //可以设置动画的属性/字段路径数组。
        //默认情况下，将使用actionclip类中具有[AnimatableParameter]属性的所有属性/字段。
        //An array of property/field paths that will be possible to animate.
        //By default all properties/fields in the actionclip class with an [AnimatableParameter] attribute will be used.
        [System.NonSerialized]
        private string[] _cachedAnimParamPaths;
        
        private string[] animatedParameterPaths {
            get { return _cachedAnimParamPaths != null ? _cachedAnimParamPaths : _cachedAnimParamPaths = AnimationDataUtility.GetAnimatableMemberPaths(this); }
        }
        
        //If the params target is not this, registration of parameters should be handled manually
        //如果参数目标不是此，则应手动处理参数注册
        virtual protected bool handleParametersRegistrationManually {
            get { return !ReferenceEquals(animatedParametersTarget, this); }
        }
        ///<summary>
        /// The target instance of the animated properties/fields. By default the instance of THIS action clip is used. Do NOT override if you don't know why! :)
        /// 动画属性/字段的目标实例。默认情况下，使用THIS动作片段的实例。如果你不知道为什么，不要覆盖！ :)
        /// </summary>
        public virtual object animatedParametersTarget {
            get { return this; }
        }
        ///<summary>
        /// The interpolation to use when blending parameters. Only relevant when useWeightInParameters is true.
        /// 混合参数时使用的插值。仅当useWeightInParameters为true时才相关。
        /// </summary>
        virtual public EaseType animatedParametersInterpolation {
            get { return EaseType.Linear; }
        }
        
        ///<summary>
        /// 参数中是否自动使用剪辑权重。
        /// </summary>
        virtual public bool useWeightInParameters {
            get { return false; }
        }
        
        ///<summary>
        /// Does the clip has any parameters?
        /// 剪辑有任何参数吗？
        /// </summary>
        public bool hasParameters {
            get { return animationData != null && animationData.isValid; }
        }
        
        ///<summary>
        /// Does the clip has any active parameters?
        /// 剪辑是否有任何活动参数？
        /// </summary>
        public bool hasActiveParameters {
            get
            {
                if ( !hasParameters || !isValid ) { return false; }
                for ( var i = 0; i < animationData.animatedParameters.Count; i++ ) {
                    if ( animationData.animatedParameters[i].enabled ) {
                        return true;
                    }
                }
                return false;
            }
        }
        
        //HOOKS 
        
        bool IDirectable.Initialize()
        {
            return OnInitialize();
        }
        public virtual bool OnInitialize() { return true; }

        void IDirectable.Enter()
        {
            SetAnimParamsSnapshot(); 
            OnEnter(); 
        }
        public virtual void OnEnter() { }
        
        void IDirectable.Update()
        {
            OnUpdate(); 
        }

        public virtual void OnUpdate()
        {
            
        }

        void IDirectable.Exit()
        {
            OnExit();
        }
        public virtual void OnExit() { }

        void IDirectable.ReverseEnter()
        {
            OnReverseEnter();
        }
        public virtual void OnReverseEnter() { }

        void IDirectable.Reverse()
        {
            RestoreAnimParamsSnapshot(); 
            OnReverse();
        }
        public virtual void OnReverse() { }

        void IDirectable.RootEnabled()
        {
            OnRootEnabled();
        }
        public virtual void OnRootEnabled() { }
        
        void IDirectable.RootDisabled()
        {
            OnRootDisabled();
        }
        public virtual void OnRootDisabled() { }
        void IDirectable.RootUpdated(float time, float previousTime)
        {
            OnRootUpdated(time, previousTime);
        }
        public virtual void OnRootUpdated(float time, float previousTime) { }
        void IDirectable.RootDestroyed()
        {
            OnRootDestroyed();
        }
        public virtual void OnRootDestroyed() { }

        
        void IDirectable.LogicUpdate()
        {
            OnLogicUpdate();
        }
        public virtual void OnLogicUpdate() { }
        

        ///<summary>After creation</summary>
        public void PostCreate(BaseActionTrack parent) {
            _parent = parent;
            CreateAnimationDataCollection();
            OnCreate();
        }

        //Validate the clip
        public void Validate() { OnAfterValidate(); }
        public void Validate(IDirector root, IDirectable parent) {
            //_parent = parent;
            hideFlags = HideFlags.HideInHierarchy;
            ValidateAnimParams();
            OnAfterValidate();
        }
        public virtual void OnCreate() { }
        
        public virtual void OnAfterValidate() { }
        
        public virtual void OnDrawGizmosSelected() { }
        public virtual void OnSceneGUI() { }

#if UNITY_EDITOR
        void IDirectable.DrawGizmos(bool selected) {
            if ( selected && actor != null && isValid ) {
                OnDrawGizmosSelected();
            }
        }

        private Dictionary<MemberInfo, Attribute[]> paramsAttributes = new Dictionary<MemberInfo, Attribute[]>();
        void IDirectable.SceneGUI(bool selected) {

            if ( !selected || actor == null || !isValid ) {
                return;
            }

            if ( hasParameters ) {

                for ( var i = 0; i < animationData.animatedParameters.Count; i++ ) {
                    var animParam = animationData.animatedParameters[i];
                    if ( !animParam.isValid || animParam.animatedType != typeof(Vector3) ) {
                        continue;
                    }
                    var m = animParam.GetMemberInfo();
                    Attribute[] attributes = null;
                    if ( !paramsAttributes.TryGetValue(m, out attributes) ) {
                        attributes = (Attribute[])m.GetCustomAttributes(false);
                        paramsAttributes[m] = attributes;
                    }

                    ITransformRefParameter link = null;
                    var animAtt = attributes.FirstOrDefault(a => a is AnimatableParameterAttribute) as AnimatableParameterAttribute;
                    if ( animAtt != null ) { //only in case parameter has been added manualy. Probably never.
                        if ( !string.IsNullOrEmpty(animAtt.link) ) {
                            try { link = ( GetType().GetField(animAtt.link).GetValue(this) as ITransformRefParameter ); }
                            catch ( Exception exc ) { Debug.LogException(exc); }
                        }
                    }

                    if ( link == null || link.useAnimation ) {

                        var space = link != null ? link.space : defaultTransformSpace;

                        var posHandleAtt = attributes.FirstOrDefault(a => a is PositionHandleAttribute) as PositionHandleAttribute;
                        if ( posHandleAtt != null ) {
                            Vector3? rotVal = null;
                            if ( !string.IsNullOrEmpty(posHandleAtt.rotationPropertyName) ) {
                                var rotProp = this.GetType().RTGetFieldOrProp(posHandleAtt.rotationPropertyName);
                                rotVal = rotProp != null ? (Vector3)rotProp.RTGetFieldOrPropValue(this) : default(Vector3);
                            }
                            DoParameterPositionHandle(animParam, space, rotVal);
                        }

                        var rotHandleAtt = attributes.FirstOrDefault(a => a is RotationHandleAttribute) as RotationHandleAttribute;
                        if ( rotHandleAtt != null ) {
                            var posProp = this.GetType().RTGetFieldOrProp(rotHandleAtt.positionPropertyName);
                            var posVal = posProp != null ? (Vector3)posProp.RTGetFieldOrPropValue(this) : default(Vector3);
                            DoParameterRotationHandle(animParam, space, posVal);
                        }

                        var trajAtt = attributes.FirstOrDefault(a => a is ShowTrajectoryAttribute) as ShowTrajectoryAttribute;
                        if ( trajAtt != null && animParam.enabled ) {
                            CurveEditor3D.Draw3DCurve(animParam, this, GetSpaceTransform(space), length / 2, length);
                        }
                    }
                }
            }

            OnSceneGUI();
        }

        protected bool DoParameterPositionHandle(AnimatedParameter animParam, TransformSpace space) {
            return SceneGUIUtility.DoParameterPositionHandle(this, animParam, space, null);
        }

        protected bool DoParameterPositionHandle(AnimatedParameter animParam, TransformSpace space, Vector3? euler) {
            return SceneGUIUtility.DoParameterPositionHandle(this, animParam, space, euler);
        }

        protected bool DoParameterRotationHandle(AnimatedParameter animParam, TransformSpace space, Vector3 position) {
            return SceneGUIUtility.DoParameterRotationHandle(this, animParam, space, position);
        }


        protected bool DoVectorPositionHandle(TransformSpace space, ref Vector3 position) {
            return SceneGUIUtility.DoVectorPositionHandle(this, space, ref position);
        }

        protected bool DoVectorPositionHandle(TransformSpace space, Vector3 euler, ref Vector3 position) {
            return SceneGUIUtility.DoVectorPositionHandle(this, space, euler, ref position);
        }

        protected bool DoVectorRotationHandle(TransformSpace space, Vector3 position, ref Vector3 euler) {
            return SceneGUIUtility.DoVectorRotationHandle(this, space, position, ref euler);
        }

#endif
        
        
        
        //SHORTCUTS 一堆快捷方法
        ///----------------------------------------------------------------------------------------------
        ///<summary>
        /// Is the root time within clip time range? A helper method.
        /// 根时间是否在剪辑时间范围内？辅助方法
        /// </summary>
        public bool RootTimeWithinRange() { return IDirectableExtensions.IsRootTimeWithinClip(this); }
        ///<summary>
        /// Transforms a point in specified space
        /// 转换指定空间中的点
        /// </summary>
        public Vector3 TransformPosition(Vector3 point, TransformSpace space) { return IDirectableExtensions.TransformPosition(this, point, space); }
        ///<summary>
        /// Inverse Transforms a point in specified space
        /// 逆变换指定空间中的点
        /// </summary>
        public Vector3 InverseTransformPosition(Vector3 point, TransformSpace space) { return IDirectableExtensions.InverseTransformPosition(this, point, space); }
        ///<summary>
        /// Transform an euler rotation in specified space and into a quaternion
        /// 将指定空间中的欧拉旋转变换为四元数
        /// </summary>
        public Quaternion TransformRotation(Vector3 euler, TransformSpace space) { return IDirectableExtensions.TransformRotation(this, euler, space); }
        ///<summary>
        /// 将指定空间中的四元数旋转转化为欧拉旋转
        /// </summary>
        public Vector3 InverseTransformRotation(Quaternion rot, TransformSpace space) { return IDirectableExtensions.InverseTransformRotation(this, rot, space); }
        ///<summary>
        /// Returns the final actor position in specified Space (InverseTransform Space)
        /// 返回指定空间（逆变换空间）中的最终演员位置
        /// </summary>
        public Vector3 ActorPositionInSpace(TransformSpace space) { return IDirectableExtensions.ActorPositionInSpace(this, space); }
        ///<summary>
        /// Returns the transform object used for specified Space transformations. Null if World Space.
        /// </summary>
        public Transform GetSpaceTransform(TransformSpace space, GameObject actorOverride = null) { return IDirectableExtensions.GetSpaceTransform(this, space, actorOverride); }
        ///<summary>
        /// Returns the previous clip in parent track
        /// 返回用于指定Space变换的变换对象。如果为世界空间，则为空。
        /// </summary>
        public BaseActionClip GetPreviousClip() { return this.GetPreviousSibling<BaseActionClip>(); }
        ///<summary>
        /// Returns the next clip in parent track
        /// 返回父track中的下一个clip
        /// </summary>
        public BaseActionClip GetNextClip() { return this.GetNextSibling<BaseActionClip>(); }
        ///<summary>
        /// The current clip weight based on blend properties and based on root current time.
        /// 基于混合属性和根当前时间的当前剪辑权重。
        /// </summary>
        public float GetClipWeight() { return GetClipWeight(root.currentTime - startTime); }
        ///<summary>
        /// The weight of the clip at specified local time based on its blend properties.
        /// 基于其混合特性的指定本地时间片段的权重。
        /// </summary>
        public float GetClipWeight(float time) { return GetClipWeight(time, this.blendIn, this.blendOut); }
        ///<summary>
        /// The weight of the clip at specified local time based on provided override blend in/out properties
        /// 基于提供的覆盖混合输入/输出特性，在指定的本地时间片段的重量
        /// </summary>
        public float GetClipWeight(float time, float blendInOut) { return GetClipWeight(time, blendInOut, blendInOut); }
        public float GetClipWeight(float time, float blendIn, float blendOut) { return this.GetWeight(time, blendIn, blendOut); }
        ///----------------------------------------------------------------------------------------------
        
        public void TryMatchSubClipLength() {
            if ( this is ISubClipContainable ) {
                length = ( this as ISubClipContainable ).subClipLength / ( this as ISubClipContainable ).subClipSpeed;
            }
        }
        
        ///<summary>
        /// Try set the clip length to match previous subclip loop if this contains a subclip at all.
        /// 如果此循环包含子剪辑，请尝试将剪辑长度设置为与之前的子剪辑循环匹配。
        /// </summary>
        public void TryMatchPreviousSubClipLoop() {
            if ( this is ISubClipContainable ) {
                length = ( this as ISubClipContainable ).GetPreviousLoopLocalTime();
            }
        }
        
        ///<summary>
        /// Try set the clip length to match next subclip loop if this contains a subclip at all.
        ///如果下一个子剪辑循环中包含子剪辑，请尝试设置剪辑长度以匹配下一个子剪辑循环。
        /// </summary>
        public void TryMatchNexSubClipLoop() {
            if ( this is ISubClipContainable ) {
                var targetLength = ( this as ISubClipContainable ).GetNextLoopLocalTime();
                var nextClip = GetNextClip();
                if ( nextClip == null || startTime + targetLength <= nextClip.startTime ) {
                    length = targetLength;
                }
            }
        }
        
        string GetParameterName<T, TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> func) {
            return ReflectionTools.GetMemberPath(func);
        }
        
        ///<summary>
        /// Get the AnimatedParameter of name. The name is usually the same as the field/property name that [AnimatableParameter] is used on.
        /// 获取name的AnimatedParameter。该名称通常与使用[AnimatableParameter]的字段/属性名称相同。
        /// </summary>
        public AnimatedParameter GetParameter<T, TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> func) { return GetParameter(GetParameterName(func)); }
        ///<summary>
        /// Get the AnimatedParameter of name. The name is usually the same as the field/property name that [AnimatableParameter] is used on.
        /// 获取name的AnimatedParameter。该名称通常与使用[AnimatableParameter]的字段/属性名称相同。
        /// </summary>
        public AnimatedParameter GetParameter(string paramName) {
            return animationData != null ? animationData.GetParameterOfName(paramName) : null;
        }
        
        ///<summary>
        /// Enable/Disable an AnimatedParameter of name
        /// 启用/禁用名称为的动画参数
        /// </summary>
        public void SetParameterEnabled<T, TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> func, bool enabled) { SetParameterEnabled(GetParameterName(func), enabled); }
        ///<summary>
        /// Enable/Disable an AnimatedParameter of name
        /// 启用/禁用名称为的动画参数
        /// </summary>
        public void SetParameterEnabled(string paramName, bool enabled) {
            var animParam = GetParameter(paramName);
            if ( animParam != null ) {
                animParam.SetEnabled(enabled, root.currentTime - startTime);
            }
        }

        ///<summary>
        /// Re-Init/Reset all existing animated parameters
        /// 重新初始化/重置所有现有的动画参数
        /// </summary>
        public void ResetAnimatedParameters() {
            if ( animationData != null ) {
                animationData.Reset();
            }
        }
        
        //Creates the animation data collection out of the fields/properties marked with [AnimatableParameter] attribute
        //从标记有[AnimatableParameter]属性的字段/属性中创建动画数据集合
        void CreateAnimationDataCollection() {

            if ( handleParametersRegistrationManually ) {
                return;
            }

            if ( animatedParameterPaths != null && animatedParameterPaths.Length != 0 ) {
                animationData = new AnimationDataCollection(this, this.GetType(), animatedParameterPaths, null);
            }
        }

        //Validate the animation parameters vs the animation data collection to be synced, adding or removing as required.
        //验证动画参数与要同步的动画数据集，根据需要添加或删除。
        void ValidateAnimParams() {

            if ( animationData != null ) {
                animationData.Validate(this);
            }

            //we don't need validation in runtime
            if ( Application.isPlaying ) {
                return;
            }

            if ( handleParametersRegistrationManually ) {
                return;
            }

            if ( animatedParameterPaths == null || animatedParameterPaths.Length == 0 ) {
                animationData = null;
                return;
            }

            //try append new
            for ( var i = 0; i < animatedParameterPaths.Length; i++ ) {
                var memberPath = animatedParameterPaths[i];
                if ( !string.IsNullOrEmpty(memberPath) ) {
                    animationData.TryAddParameter(this, this.GetType(), memberPath, null);
                }
            }

            //cleanup
            foreach ( var animParam in animationData.animatedParameters.ToArray() ) {
                if ( !animParam.isValid ) {
                    animationData.RemoveParameter(animParam);
                    continue;
                }

                if ( !animatedParameterPaths.Contains(animParam.parameterName) ) {
                    animationData.RemoveParameter(animParam);
                    continue;
                }
            }
        }

        //Set an animation snapshot for all parameters
        //为所有参数设置动画快照
        void SetAnimParamsSnapshot() {
            if ( hasParameters ) {
                animationData.SetVirtualTransformParent(GetSpaceTransform(defaultTransformSpace));
                animationData.SetSnapshot();
            }
        }

        //TODO 动画的改删了，应该全部交给Animacer处理得了
        //Update the animation parameters, setting their evaluated values
        //更新动画参数，设置其评估值
        void UpdateAnimParams(float time, float previousTime) {
            if ( hasParameters ) {
                var ease = 1f;
                if ( useWeightInParameters ) {
                    //var clipWeight = GetClipWeight(time);
                    //ease = animatedParametersInterpolation == EaseType.Linear ? clipWeight : Easing.Ease(animatedParametersInterpolation, 0, 1, clipWeight);
                }
                //animationData.Evaluate(time, previousTime, ease);
            }
        }

        //Restore the animation snapshot on all parameters
        void RestoreAnimParamsSnapshot() {
            if ( hasParameters ) {
                animationData.RestoreSnapshot();
            }
        }
        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
        
#if UNITY_EDITOR

        ///<summary>Unity callback.</summary>
        protected void OnValidate() { OnEditorValidate(); }

        ///<summary>Show clip GUI contents</summary>
        public void ShowClipGUI(Rect rect) { OnClipGUI(rect); }
        ///<summary>This is called outside of the clip for UI on the the left/right sides of the clip.</summary>
        public void ShowClipGUIExternal(Rect left, Rect right) { OnClipGUIExternal(left, right); }

        public void ClipLengthChanged() { OnClipLengthChanged(); }
        
        ///<summary>Override for extra clip GUI contents.</summary>
        virtual protected void OnClipGUI(Rect rect) { }
        ///<summary>Override for extra clip GUI contents outside of clip.</summary>
        virtual protected void OnClipGUIExternal(Rect left, Rect right) { }
        ///<summary>Override to validate things in editor.</summary>
        virtual protected void OnEditorValidate() { }
        
        virtual protected void OnClipLengthChanged() { }

        ///----------------------------------------------------------------------------------------------
#endif
    }
}