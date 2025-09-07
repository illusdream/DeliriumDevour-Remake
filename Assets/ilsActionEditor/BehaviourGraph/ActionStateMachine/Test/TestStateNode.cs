using System.Collections.Generic;
using ilsFramework.Core;
using UnityEngine;

namespace ilsActionEditor.Test
{
    public class TestStateNode : AFSMStateNode
    {
        public bool Move;
        
        public override void OnInit()
        {
            base.OnInit();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Move && BlackBoard is not null)
            {
                BlackBoard.GetValue<AFSMRunner>("Runner").transform.position += new Vector3(1,0,1) * deltaTime;
            }
            base.OnUpdate(deltaTime);
        }
        
        
    }
}