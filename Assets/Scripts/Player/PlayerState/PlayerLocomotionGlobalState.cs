using ilsActionEditor;
using UnityEngine;

[NodeMenuItem("Player/GlobalState")]
public class PlayerLocomotionGlobalState : AFSMGlobalState
{
        [SerializeReference]
        [ExitTranslation]
        public PlayerDashTranslation Dash;
        
        [SerializeReference]
        [ExitTranslation]
        public PlayerJumpTranslation Jump;
}