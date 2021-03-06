using System;
using En.Gen.Crankshaft.Fork;

namespace En.Gen.Crankshaft
{
    public interface IResolveMiddlewareFactory
    {
        Func<TMiddleware> ResolveFactory<TMiddleware>()
            where TMiddleware : IMiddleware;

        Func<IPipeline<object>, IPipeline<object>, TMiddleware> ResolveForkFactory<TMiddleware>()
            where TMiddleware : ForkedMiddleware;
    }
}