using System.Collections.Generic;

public class BaseProcessorPipelineContainer<T> where T : BaseProcessorPipeline
{
        
        public List<string> GetAllProcessors()
        {
                return ProcessorPipelineManager.Instance.GetPipelineConfig(typeof(T))?.Pipelines;
        }
}