using System;
using En.Gen.Crankshaft.Fork;

namespace En.Gen.Crankshaft
{
    public interface IBuildPipeline
    {
        IBuildPipeline Use<TMiddleware>()
            where TMiddleware : IMiddleware;

        IBuildPipeline Fork<TForkedMiddleware>(Action<IBuildPipeline> buildLeft, Action<IBuildPipeline> buildRight)
            where TForkedMiddleware : ForkedMiddleware;

        IPipeline Build();
    }

    public interface IBuildPipeline<in TPayload> : IBuildPipeline
    {
        new IBuildPipeline<TPayload> Use<TMiddleware>()
            where TMiddleware : IMiddleware;

        new IBuildPipeline<TPayload> Fork<TForkedMiddleware>(Action<IBuildPipeline> buildLeft, Action<IBuildPipeline> buildRight)
            where TForkedMiddleware : ForkedMiddleware;

        new IPipeline<TPayload> Build();
    }
}