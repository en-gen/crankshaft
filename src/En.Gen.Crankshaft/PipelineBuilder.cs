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
            var leftBuilder = new PipelineBuilder(FactoryResolver);
            var rightBuilder = new PipelineBuilder(FactoryResolver);

            buildLeft(leftBuilder);
            buildRight(rightBuilder);

            var leftPipeline = leftBuilder.Build();
            var rightPipeline = rightBuilder.Build();

            var createFork = FactoryResolver.ResolveForkFactory<TForkedMiddleware>();

            Middleware.Add(() => createFork(leftPipeline, rightPipeline));
            return this;
        }

        public virtual IPipeline Build()
        {
            return new Pipeline(Middleware.ToList());
        }
    }

    public class PipelineBuilder<TPayload> : PipelineBuilder, IBuildPipeline<TPayload>
    {
        public PipelineBuilder(IResolveMiddlewareFactory factoryResolver) :
            base(factoryResolver)
        {
        }

        public new IBuildPipeline<TPayload> Use<TMiddleware>()
            where TMiddleware : IMiddleware
        {
            base.Use<TMiddleware>();
            return this;
        }

        public new IBuildPipeline<TPayload> Fork<TForkedMiddleware>(Action<IBuildPipeline> buildLeft, Action<IBuildPipeline> buildRight)
            where TForkedMiddleware : ForkedMiddleware
        {
            base.Fork<TForkedMiddleware>(buildLeft, buildRight);
            return this;
        }

        public new IPipeline<TPayload> Build()
        {
            return new Pipeline<TPayload>(Middleware.ToList());
        }
    }
}
