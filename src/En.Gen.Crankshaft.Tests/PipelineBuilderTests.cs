using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using En.Gen.Crankshaft.Fork;
using Moq;
using Xunit;

namespace En.Gen.Crankshaft.Tests
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void Build__Given_RegisteredMiddleware__Then_CreatePipeline()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            var mockMiddleware = Enumerable.Range(0, 3)
                .Select(i =>
                {
                    var mock = new Mock<IMiddleware>();
                    mock
                        .Setup(x => x.BeforeNext(environment, payload))
                        .Returns(Task.FromResult(true));
                    return mock;
                })
                .ToArray();

            var index = 0;
            var mockFactoryResolver = new Mock<IResolveMiddlewareFactory>();
            mockFactoryResolver
                .Setup(x => x.ResolveFactory<IMiddleware>())
                .Returns(() => mockMiddleware[index++].Object);

            var subject = new PipelineBuilder(mockFactoryResolver.Object);
            subject
                .Use<IMiddleware>()
                .Use<IMiddleware>()
                .Use<IMiddleware>();
            var pipeline = subject.Build<object>();
            pipeline.Process(payload);

            foreach (var mock in mockMiddleware)
            {
                mock.Verify(x => x.BeforeNext(environment, payload), Times.Once);
            }
        }

        [Fact]
        public void Build__When_MiddlewareUnregistered__Then_ThrowException()
        {
            var subject = new PipelineBuilder(Mock.Of<IResolveMiddlewareFactory>());
            Assert.Throws<Exception>(() => subject.Use<IMiddleware>());
        }

        [Fact]
        public void BuildGeneric__Given_RegisteredMiddleware__Then_CreatePipeline()
        {
            var environment = new Dictionary<string, object>();
            var payload = "TEST";

            var mockMiddleware = Enumerable.Range(0, 3)
                .Select(i =>
                {
                    var mock = new Mock<IMiddleware>();
                    mock
                        .Setup(x => x.BeforeNext(environment, payload))
                        .Returns(Task.FromResult(true));
                    return mock;
                })
                .ToArray();

            var index = 0;
            var mockFactoryResolver = new Mock<IResolveMiddlewareFactory>();
            mockFactoryResolver
                .Setup(x => x.ResolveFactory<IMiddleware>())
                .Returns(() => mockMiddleware[index++].Object);

            var subject = new PipelineBuilder(mockFactoryResolver.Object);
            subject
                .Use<IMiddleware>()
                .Use<IMiddleware>()
                .Use<IMiddleware>();
            var pipeline = subject.Build<object>();
            pipeline.Process(payload);

            foreach (var mock in mockMiddleware)
            {
                mock.Verify(x => x.BeforeNext(environment, payload), Times.Once);
            }
        }

        [Fact]
        public async Task Fork__Given_RegisteredForkedMiddleware__Then_ConfigureForksAndAddForkMiddleware()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            Mock<ForkedMiddleware> mockForkedMiddleware = null;
            var mockFactoryResolver = new Mock<IResolveMiddlewareFactory>();
            mockFactoryResolver
                .Setup(x => x.ResolveForkFactory<ForkedMiddleware>())
                .Returns((left, right) =>
                {
                    mockForkedMiddleware = new Mock<ForkedMiddleware>(Tuple.Create(left, right));
                    mockForkedMiddleware
                        .Setup(x => x.BeforeNext(environment, payload))
                        .Returns(Task.FromResult(true));
                    return mockForkedMiddleware.Object;
                });

            var leftBuilderConfigured = false;
            var rightBuilderConfigured = false;
            var mockConfigureLeft = new Action<IBuildPipeline>(builder => leftBuilderConfigured = true);
            var mockConfigureRight = new Action<IBuildPipeline>(builder => rightBuilderConfigured = true);

            var subject = new PipelineBuilder(mockFactoryResolver.Object);
            var forkResult = subject.Fork<ForkedMiddleware>(mockConfigureLeft, mockConfigureRight);

            Assert.Same(subject, forkResult);

            Assert.True(leftBuilderConfigured);
            Assert.True(rightBuilderConfigured);

            var processResult = await subject
                .Build<object>()
                .Process(payload);

            Assert.True(processResult);
            Assert.NotNull(mockForkedMiddleware);
            mockForkedMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
        }

        [Fact]
        public async Task ForkGeneric__Given_RegisteredForkedMiddleware__Then_ConfigureForksAndAddForkMiddleware()
        {
            var environment = new Dictionary<string, object>();
            var payload = "TEST";

            Mock<ForkedMiddleware> mockForkedMiddleware = null;
            var mockFactoryResolver = new Mock<IResolveMiddlewareFactory>();
            mockFactoryResolver
                .Setup(x => x.ResolveForkFactory<ForkedMiddleware>())
                .Returns((left, right) =>
                {
                    mockForkedMiddleware = new Mock<ForkedMiddleware>(Tuple.Create(left, right));
                    mockForkedMiddleware
                        .Setup(x => x.BeforeNext(environment, payload))
                        .Returns(Task.FromResult(true));
                    return mockForkedMiddleware.Object;
                });

            var leftBuilderConfigured = false;
            var rightBuilderConfigured = false;
            var mockConfigureLeft = new Action<IBuildPipeline>(builder => leftBuilderConfigured = true);
            var mockConfigureRight = new Action<IBuildPipeline>(builder => rightBuilderConfigured = true);

            var subject = new PipelineBuilder(mockFactoryResolver.Object);
            var forkResult = subject.Fork<ForkedMiddleware>(mockConfigureLeft, mockConfigureRight);

            Assert.Same(subject, forkResult);

            Assert.True(leftBuilderConfigured);
            Assert.True(rightBuilderConfigured);

            var processResult = await subject
                .Build<object>()
                .Process(payload);

            Assert.True(processResult);
            Assert.NotNull(mockForkedMiddleware);
            mockForkedMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
        }
    }
}
