using System;
using System.Collections.Generic;

namespace ilsActionEditor
{
    [NodeColor(0f,0f,1f,1f)]
    public class AFSMGlobalState : AFSMStateNode
    {
        [ShowInGraph]
        public int Priority;
        public override List<Type> GetAllCanUseEntryTransitions()
        {
            var result =base.GetAllCanUseEntryTransitions();
            result.Clear();
            return result;
        }
    }
}