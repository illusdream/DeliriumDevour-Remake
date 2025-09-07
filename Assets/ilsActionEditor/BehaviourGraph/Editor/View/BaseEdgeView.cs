using ilsFramework.Core;
using UnityEngine.UIElements;
using EdgeView = UnityEditor.Experimental.GraphView.Edge;
namespace ilsActionEditor.Editor
{
    public class BaseEdgeView :EdgeView
    {
        public BaseEdgeView()
        {
        }
        //这里存储两边的属性，用来删除或者建立链接
        
        public override void Select(VisualElement selectionContainer, bool additive)
        {
            base.Select(selectionContainer, additive);
        }

    }
}