using System.Collections.Generic;
using ilsFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

[AutoBuildOrLoadConfig("ProcessorPipelineConfig/ProcessorPipelineConfig")]
public class ProcessorPipelineConfig : ConfigScriptObject
{
    public override string ConfigName => "ProcessorPipeline";
    
    [SerializeField] [ListDrawerSettings(ShowFoldout = false)]
    public List<SingleProcessorPipelineConfig> pipelineConfigs = new List<SingleProcessorPipelineConfig>();

    public override List<(string, object)> AddMenuItem(string prefix)
    {
        var result = base.AddMenuItem(prefix);
        
        pipelineConfigs.ForEach(pc =>
        {
            result.Add((prefix +"/"+ pc.Name,pc));
        });
        return result;
    }
}