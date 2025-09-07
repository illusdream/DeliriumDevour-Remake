using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

//数值相关的，非数值暂时不考虑
public class OldStat<T,TValue> : BaseProcessorPipelineContainer<T> where T : BaseProcessorPipeline
{
        public TValue BaseValue;
        
        [ShowInInspector]
        Dictionary<string,List<StatModifier<TValue>>> modifiers = new Dictionary<string,List<StatModifier<TValue>>>();
        
        public OldStat(TValue baseValue)
        {
                BaseValue = baseValue;
                var allProcessorPipelineCells = GetAllProcessors();
                allProcessorPipelineCells.ForEach(cell =>
                {
                        modifiers[cell] = new List<StatModifier<TValue>>();
                });
        }

        public TValue Value => GetValue();
        
        public TValue GetValue()
        {
                var currentValue = BaseValue;
                GetAllProcessors().ForEach(cell =>
                {
                        modifiers[cell].ForEach(modifier =>
                        {
                                currentValue = modifier.Func(currentValue);
                        });
                });
                return currentValue;
        }
}