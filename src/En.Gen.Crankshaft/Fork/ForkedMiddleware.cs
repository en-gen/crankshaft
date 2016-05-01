using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft.Fork
{
    public abstract class ForkedMiddleware : IMiddleware
    {
        protected IPipeline<object> LeftPipeline { get; }
        protected IPipeline<object> RightPipeline { get; }
        
        protected ForkedMiddleware(Tuple<IPipeline<object>, IPipeline<object>> pipelines)
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

        public virtual async Task<bool> BeforeNext(IDictionary<string, object> environment, object payload)
        {
            var pipeline = ChoosePipeline(payload) as IForkedPipeline;
            if (pipeline != null)
            {
                return await pipeline.Process(environment, payload);
            }
            return true;
        }

        protected abstract IPipeline<object> ChoosePipeline(object payload);

        public abstract Task AfterNext(IDictionary<string, object> environment, object payload);
    }
}
