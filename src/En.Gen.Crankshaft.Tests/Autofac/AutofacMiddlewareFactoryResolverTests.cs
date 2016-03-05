using System;
using System.Threading.Tasks;
using Autofac;
using En.Gen.Crankshaft.Autofac;
using En.Gen.Crankshaft.Fork;
using Moq;
using Xunit;

namespace En.Gen.Crankshaft.Tests.Autofac
{
    public class AutofacMiddlewareFactoryResolverTests
    {
        [Fact]
        public void ResolveFactory__Given_RegisteredMiddleware__Then_ReturnMiddlewareFunc()
        {
            var expectedMiddleware = Mock.Of<IMiddleware>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(expectedMiddleware);

            var subject = new AutofacMiddlewareFactoryResolver(containerBuilder.Build());
            var result = subject.ResolveFactory<IMiddleware>();
            var middleware = result();

            Assert.Same(expectedMiddleware, middleware);
        }

        [Fact]
        public void ResolveForkFactory__Given_RegisteredMiddleware__Then_ReturnForkMiddlewareFunc()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<TestableForkedMiddleware>();

            var subject = new AutofacMiddlewareFactoryResolver(containerBuilder.Build());
            var result = subject.ResolveForkFactory<TestableForkedMiddleware>();

            var expectedLeftPipeline = Mock.Of<IPipeline>();
            var expectedRightPipeline = Mock.Of<IPipeline>();
            var middleware = result(expectedLeftPipeline, expectedRightPipeline);

            Assert.NotNull(middleware);
            Assert.IsType<TestableForkedMiddleware>(middleware);
            Assert.Same(expectedLeftPipeline, middleware.TestLeftPipeline);
            Assert.Same(expectedRightPipeline, middleware.TestRightPipeline);
        }

        internal class TestableForkedMiddleware : ForkedMiddleware
        {
            public IPipeline TestLeftPipeline => LeftPipeline;
            public IPipeline TestRightPipeline => RightPipeline;
            
            public TestableForkedMiddleware(Tuple<IPipeline, IPipeline> pipelines) :
                base(pipelines)
            {
            }

            protected override IPipeline ChoosePipeline(object payload)
            {
                throw new NotImplementedException();
            }

            public override Task PostProcess(object payload)
            {
                throw new NotImplementedException();
            }
        }
    }
}
