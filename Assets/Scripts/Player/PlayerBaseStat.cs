using Animancer;
using ilsActionEditor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Player/Player Base Stat", fileName = "Player Base Stat")]
public class PlayerBaseStat :  ScriptableObject
{
        [FoldoutGroup("Ground")]
        
        
        [FoldoutGroup("Ground/Idle")] public ClipTransition Idle_Left_Anim;
        [FoldoutGroup("Ground/Idle")] public ClipTransition Idle_Right_Anim;

        [FoldoutGroup("Ground/Move")] public float BaseMoveSpeed;
        [FoldoutGroup("Ground/Move")] public float Acc;
        [FoldoutGroup("Ground/Move")][LabelText("高速缓冲的动作的阈值")] public float MoveBufferStopSpeed;
        [FoldoutGroup("Ground/Move")] public float BaseTurnMaxQSpeed;
        [FoldoutGroup("Ground/Move")] public float BaseGravityScale;
        [FoldoutGroup("Ground/Move")] public ClipTransition Move_Start_Anim;
        [FoldoutGroup("Ground/Move/Turn")] public TransitionAsset Turn180L_L;
        [FoldoutGroup("Ground/Move/Turn")] public TransitionAsset Turn180L_R;
        [FoldoutGroup("Ground/Move/Turn")] public TransitionAsset Turn180R_L;
        [FoldoutGroup("Ground/Move/Turn")] public TransitionAsset Turn180R_R;
        [FoldoutGroup("Ground/Move")] public TransitionAsset Move_Loop_Anim;
        [FoldoutGroup("Ground/Move")] public StringAsset Move_Speed_Paramater;
        [FoldoutGroup("Ground/Move")] public StringAsset Move_DeltaArc_Paramater;
        [FoldoutGroup("Ground/Move")] public TransitionAsset Move_End_Left_Anim;
        [FoldoutGroup("Ground/Move")] public TransitionAsset Move_End_Left_Buffer_Anim;
        [FoldoutGroup("Ground/Move")] public TransitionAsset Move_End_Right_Anim;
        [FoldoutGroup("Ground/Move")] public TransitionAsset Move_End_Right_Buffer_Anim;


        [FoldoutGroup("Dash")] public ActionTimeline DashActionTimeLine;
        [FoldoutGroup("Dash")] public float DashVelocityMultiplier;
        [FoldoutGroup("Dash")] public float DashBaseVelocity;
        
        
        [FoldoutGroup("Jump")] public TransitionAsset InAir_L;
        [FoldoutGroup("Jump")] public TransitionAsset InAir_R;
}