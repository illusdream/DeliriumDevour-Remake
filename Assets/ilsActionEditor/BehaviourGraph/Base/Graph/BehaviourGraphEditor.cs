using System;
using ilsActionEditor.Node;
using ilsFramework.Core;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace ilsActionEditor.BehaviourGraph
{
    [CustomNodeGraphEditor(typeof(BaseBehaviourGraph))]
    public class BehaviourGraphEditor : XNodeEditor.NodeGraphEditor
    {
        #region UI布局

        public readonly float FieldAreaMaxWidth = 200f;
        public readonly float FieldAreaMinWidthPercent = 0.2f;
        public readonly float FieldAreaMinWidth = 100f;

        public Rect FieldAreaRect;
        
        #endregion

        public override string GetNodeMenuName(Type type)
        {
            if (typeof(BaseBehaviourNode).IsAssignableFrom(type)) {
                return base.GetNodeMenuName(type);
            } else return null;
        }




        public override void OnGUI()
        {
                
            
            

            DrawFieldArea();
            base.OnGUI();
        }


        private void DrawFieldArea()
        {
            var width = Mathf.Min((window.position.width * FieldAreaMinWidthPercent), FieldAreaMaxWidth);
            width = Mathf.Max(FieldAreaMinWidth, width);
            FieldAreaRect = new Rect(0, 0, width, window.position.height);
            
            GUI.color = Color.grey;
            GUI.Box(FieldAreaRect, "");
            
            GUI.BeginGroup(FieldAreaRect);
            
            GUI.EndGroup();
        }
        
    }
}