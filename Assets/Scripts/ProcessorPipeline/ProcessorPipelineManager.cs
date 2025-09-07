using System;
using System.Collections.Generic;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class ProcessorPipelineManager : ManagerSingleton<ProcessorPipelineManager>,IAssemblyForeach
{
    [ShowInInspector]
    Dictionary<string,SingleProcessorPipelineConfig> pipelines = new Dictionary<string, SingleProcessorPipelineConfig>();
    [ShowInInspector]
    Dictionary<Type, SingleProcessorPipelineConfig> pipelineTypes = new Dictionary<Type, SingleProcessorPipelineConfig>();
    public override void OnInit()
    {

    }
    
    public void ForeachCurrentAssembly(Type[] types)
    {
        var config = Config.GetConfig<ProcessorPipelineConfig>();
        config.pipelineConfigs.ForEach(pc =>
        {
            pipelines.Add(pc.Name, pc);
        });
        types.ForEach((t) =>
        {
            if (typeof(BaseProcessorPipeline).IsAssignableFrom(t) && !t.IsAbstract)
            {
                var instance = Activator.CreateInstance(t) as BaseProcessorPipeline;
                if (pipelines.TryGetValue(instance.Name,out var value))
                {
                    pipelineTypes[t] = value;
                }
            }
        });
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnLateUpdate()
    {
        
    }

    public override void OnLogicUpdate()
    {
       
    }

    public override void OnFixedUpdate()
    {
       
    }

    public override void OnDestroy()
    {
        
    }

    public override void OnDrawGizmos()
    {
       
    }

    public override void OnDrawGizmosSelected()
    {
       
    }

    public SingleProcessorPipelineConfig GetPipelineConfig(string pipelineName)
    {
        return pipelines.GetValueOrDefault(pipelineName);
    }

    public SingleProcessorPipelineConfig GetPipelineConfig(Type pipelineType)
    {
        return pipelineTypes.GetValueOrDefault(pipelineType);
    }


}