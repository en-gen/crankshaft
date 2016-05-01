using System;
using System.Collections.Generic;
using System.Linq;
using En.Gen.Crankshaft.Fork;

namespace En.Gen.Crankshaft
{
    public class PipelineBuilder : IBuildPipeline
    {
        protected IList<Func<IMiddleware>> Middleware { get; }
        protected IResolveMiddlewareFactory FactoryResolver { get; }
        
        public PipelineBuilder(IResolveMiddlewareFactory factoryResolver)
        {
            Middleware = new List<Func<IMiddleware>>();
            FactoryResolver = factoryResolver;
        }

        public IBuildPipeline Use<TMiddleware>()
            where TMiddleware : IMiddleware
        {
            var factory = FactoryResolver.ResolveFactory<TMiddleware>() as Func<IMiddleware>;
            if (factory == null)
            {
                throw new Exception("failed to cast func");
            }
            Middleware.Add(factory);
            return this;
        }

        public IBuildPipeline Fork<TForkedMiddleware>(Action<IBuildPipeline> buildLeft, Action<IBuildPipeline> buildRight)
            where TForkedMiddleware : ForkedMiddleware
        {
            var leftBuilder = new ForkedPipelineBuilder(FactoryResolver);
            var rightBuilder = new ForkedPipelineBuilder(FactoryResolver);

            buildLeft(leftBuilder);
            buildRight(rightBuilder);

            var leftPipeline = leftBuilder.Build<object>();
            var rightPipeline = rightBuilder.Build<object>();

            var createFork = FactoryResolver.ResolveForkFactory<TForkedMiddleware>();

            Middleware.Add(() => createFork(leftPipeline, rightPipeline));
            return this;
        }

        public virtual IPipeline<TPayload> Build<TPayload>()
        {
            return new Pipeline<TPayload>(Middleware.ToList());
        }
    }
}
