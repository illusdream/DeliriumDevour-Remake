using UnityEngine;

namespace ilsActionEditor
{

    public interface ITransformRefParameter
    {
        Transform transform { get; }
        TransformSpace space { get; }
        bool useAnimation { get; }
    }
}