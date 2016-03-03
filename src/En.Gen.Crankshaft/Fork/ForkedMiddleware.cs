using System;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft.Fork
{
    public abstract class ForkedMiddleware : IMiddleware
    {
        protected IPipeline LeftPipeline { get; }
        protected IPipeline RightPipeline { get; }
        
        protected ForkedMiddleware(Tuple<IPipeline, IPipeline> pipelines)
        {
            if(pipelines == null)
                throw new ArgumentNullException(nameof(pipelines));
            if(pipelines.Item1 == null)
                throw new ArgumentNullException(nameof(pipelines.Item1));
            if(pipelines.Item2 == null)
                throw new ArgumentNullException(nameof(pipelines.Item2));

            LeftPipeline = pipelines.Item1;
            RightPipeline = pipelines.Item2;
        }

        public virtual async Task<bool> Process(object payload)
        {
            var pipeline = ChoosePipeline(payload);
            if (pipeline != null)
            {
                return await pipeline.Process(payload);
            }
            return await Task.FromResult(true);
        }

        protected abstract IPipeline ChoosePipeline(object payload);

        public abstract Task PostProcess(object payload);
    }
}
