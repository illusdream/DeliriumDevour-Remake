using ilsActionEditor;
using Sirenix.OdinInspector;
using StatModel;
using UnityEngine;
using UnityEngine.Serialization;
using ShowIfG = Sirenix.OdinInspector.ShowIfGroupAttribute;

public  class BasePlayerState : AFSMStateNode
{
        //将一些基础的字段移到这里，防止每次都要写
        [LabelText("显示外部组件")]
        [LeftToggle]
        [SerializeField]
        [FoldoutGroup("外部组件")]
        private bool showOuterComponent;

       
        private string s;
        
        #region 属性部分
        [FoldoutGroup("外部组件/组件/属性"),ShowIfG("外部组件/组件",Condition = "showOuterComponent")]
        public BaseActorMoveStatModel MoveStatModel;
        [FoldoutGroup("外部组件/组件/属性")]
        public BaseActorActionStatModel ActionStatModel;

        #endregion

        #region 功能组件
        [FoldoutGroup("外部组件/组件/功能")]
        public Transform playerTransform;
        [FoldoutGroup("外部组件/组件/功能")]
        public AnimationHandler animationHandler;
        [FoldoutGroup("外部组件/组件/功能")]
        public PlayerInputHandler playerInputHandler;
        [FormerlySerializedAs("controller")] [FoldoutGroup("外部组件/组件/功能")]
        public PlayerController playerController;

        #endregion
        
        #region 感知部分
        [FoldoutGroup("外部组件/组件/感知器")]
        public OnGroundSensor onGroundSensor;
        #endregion

        public override void OnInit()
        {
                MoveStatModel = BlackBoard.GetValue<BaseActorMoveStatModel>(BaseActorMoveStatModel.BlackBoardKey);
                ActionStatModel = BlackBoard.GetValue<BaseActorActionStatModel>(BaseActorActionStatModel.BlackBoardKey);
                
                playerTransform = BlackBoard.GetValue<Transform>("MainTransform");
                animationHandler = BlackBoard.GetValue<AnimationHandler>(AnimationHandler.BlackBoardKey);
                playerInputHandler = BlackBoard.GetValue<PlayerInputHandler>("PlayerInputHandler");
                playerController = BlackBoard.GetValue<PlayerController>("PlayerController");
                
                onGroundSensor = BlackBoard.GetValue<OnGroundSensor>(OnGroundSensor.BlackBoardKey);
                
                base.OnInit();
        }
}