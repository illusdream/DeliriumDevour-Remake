using System;
using Animancer;
using ilsActionEditor;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorLocomotionSets", menuName = "Actor/Animation")]
public class ActorLocomotionSets : ScriptableObject,IBindingTarget
{
    public string BindingName=>"ActorLocomotionSets";
    public Type BindingType { get; }
    
    [FoldoutGroup("Ground")]
        
        
    [FoldoutGroup("Ground/Idle")] public ClipTransition Idle_Left_Anim;
    [FoldoutGroup("Ground/Idle")] public ClipTransition Idle_Right_Anim;
    
    [FoldoutGroup("Ground/Move/Parameter")][LabelText("高速缓冲的动作的阈值")] public float MoveBufferStopSpeed;
    [FoldoutGroup("Ground/Move/Parameter")] public StringAsset Move_DeltaArc_Parameter;
    [FoldoutGroup("Ground/Move/Parameter")] public StringAsset Move_Speed_Parameter;
    
    [FoldoutGroup("Ground/Move")] public TransitionAsset Move_Loop_Anim;

    [FoldoutGroup("Ground/Move/Stop")] public TransitionAsset Move_End_Left_Anim;
    [FoldoutGroup("Ground/Move/Stop")] public TransitionAsset Move_End_Left_Buffer_Anim;
    [FoldoutGroup("Ground/Move/Stop")] public TransitionAsset Move_End_Right_Anim;
    [FoldoutGroup("Ground/Move/Stop")] public TransitionAsset Move_End_Right_Buffer_Anim;


    [FoldoutGroup("Dash")] public ActionTimeline DashActionTimeLine;
        
        
    [FoldoutGroup("InAir")] public TransitionAsset InAir_L;
    [FoldoutGroup("InAir")] public TransitionAsset InAir_R;
}