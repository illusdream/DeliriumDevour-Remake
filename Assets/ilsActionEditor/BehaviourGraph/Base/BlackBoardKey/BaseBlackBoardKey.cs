using System;

namespace ilsActionEditor
{
    [Serializable]
    public abstract class BaseBlackBoardKey<T>
    {
        public abstract void GetTargetKey(out string targetKey,out Type targetKeyType);
    }
}