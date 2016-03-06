using System;
using Autofac;
using En.Gen.Crankshaft.Fork;

namespace En.Gen.Crankshaft.Autofac
{
    public class AutofacMiddlewareFactoryResolver : IResolveMiddlewareFactory
    {
        protected IComponentContext Container { get; }

        public AutofacMiddlewareFactoryResolver(IComponentContext container)
        {
            Container = container;
        }

        public Func<TMiddleware> ResolveFactory<TMiddleware>()
            where TMiddleware : IMiddleware
        {
            return Container.Resolve<Func<TMiddleware>>();
        }

        public Func<IPipeline<object>, IPipeline<object>, TMiddleware> ResolveForkFactory<TMiddleware>()
            where TMiddleware : ForkedMiddleware
        {
            return (leftPipeline, rightPipeline) => Container.Resolve<TMiddleware>(
                new TypedParameter(typeof(Tuple<IPipeline<object>, IPipeline<object>>), Tuple.Create(leftPipeline, rightPipeline)));
        }
    }
}
