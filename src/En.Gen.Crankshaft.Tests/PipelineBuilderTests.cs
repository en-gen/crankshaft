using System;
using System.Linq;
using System.Threading.Tasks;
using En.Gen.Crankshaft.Fork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace En.Gen.Crankshaft.Tests
{
    [TestClass]
    public class PipelineBuilderTests
    {
        [TestMethod]
        public void Build__Given_RegisteredMiddleware__Then_CreatePipeline()
        {
            var payload = new object();

            var mockMiddleware = Enumerable.Range(0, 3)
                .Select(i =>
                {
                    var mock = new Mock<IMiddleware>();
                    mock
                        .Setup(x => x.Process(payload))
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
            var pipeline = subject.Build();
            pipeline.Process(payload);

            foreach (var mock in mockMiddleware)
            {
                mock.Verify(x => x.Process(payload), Times.Once);
            }
        }

        [TestMethod]
        public async Task Fork__Given_RegisteredForkedMiddleware__Then_ConfigureForksAndAddForkMiddleware()
        {
            var payload = new object();

            Mock<ForkedMiddleware> mockForkedMiddleware = null;
            var mockFactoryResolver = new Mock<IResolveMiddlewareFactory>();
            mockFactoryResolver
                .Setup(x => x.ResolveForkFactory<ForkedMiddleware>())
                .Returns((left, right) =>
                {
                    mockForkedMiddleware = new Mock<ForkedMiddleware>(Tuple.Create(left, right));
                    mockForkedMiddleware
                        .Setup(x => x.Process(payload))
                        .Returns(Task.FromResult(true));
                    return mockForkedMiddleware.Object;
                });

            var leftBuilderConfigured = false;
            var rightBuilderConfigured = false;
            var mockConfigureLeft = new Action<IBuildPipeline>(builder => leftBuilderConfigured = true);
            var mockConfigureRight = new Action<IBuildPipeline>(builder => rightBuilderConfigured = true);

            var subject = new PipelineBuilder(mockFactoryResolver.Object);
            var forkResult = subject.Fork<ForkedMiddleware>(mockConfigureLeft, mockConfigureRight);

            Assert.AreSame(subject, forkResult);

            Assert.IsTrue(leftBuilderConfigured);
            Assert.IsTrue(rightBuilderConfigured);

            var processResult = await subject
                .Build()
                .Process(payload);

            Assert.IsTrue(processResult);
            Assert.IsNotNull(mockForkedMiddleware);
            mockForkedMiddleware.Verify(x => x.Process(payload), Times.Once);
        }
    }
}
