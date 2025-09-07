using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SingleProcessorPipelineConfig
{
        public string Name;
        public List<string> Pipelines;
}

public class SingleProcessorPipelineConfigContainer : ScriptableObject
{
        [ShowInInspector]
        public SingleProcessorPipelineConfig Config;
}