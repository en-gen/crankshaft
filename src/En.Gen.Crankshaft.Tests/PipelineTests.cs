using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using En.Gen.Crankshaft.Fork;
using Moq;
using Xunit;

namespace En.Gen.Crankshaft.Tests
{
    public class PipelineTests
    {
        [Fact]
        public async Task Process__Given_NoMiddleware__Then_DoNotProcess()
        {
            var payload = new object();
            
            var middleware = new List<Func<IMiddleware>>();

            var subject = new Pipeline<object>(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__Then_ProcessPayload()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.BeforeNext(environment, payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> {() => mockMiddleware.Object};

            var subject = new Pipeline<object>(middleware);
            var result = await subject.Process(payload);
            
            Assert.True(result);
            mockMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__When_ProcessSuccess__Then_PostProcessPayload()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.BeforeNext(environment, payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline<object>(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
            mockMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Once);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__When_ProcessFail__Then_DoNotPostProcess()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.BeforeNext(environment, payload))
                .Returns(Task.FromResult(false));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline<object>(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);
            mockMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Never);
        }

        [Fact]
        public async Task Process__Given_MultipleMiddleware__When__AllSuccess__Then_AllMiddlewareProcessPayload_And_AllMiddlewarePostProcessPayload()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();
            
            var callOrder = new List<int>();

            var mockMiddlewares = Enumerable.Range(0, 3)
                .Select(x =>
                {
                    var mockMiddleware = new Mock<IMiddleware>();
                    mockMiddleware
                        .Setup(mock => mock.BeforeNext(environment, payload))
                        .Returns(Task.FromResult(true))
                        .Callback(() => callOrder.Add(x));
                    return mockMiddleware;
                })
                .ToArray();

            var middlewares = mockMiddlewares
                .Select<Mock<IMiddleware>, Func<IMiddleware>>(x => () => x.Object)
                .ToList();

            var subject = new Pipeline<object>(middlewares);
            var result = await subject.Process(payload);
            
            Assert.True(result);
            Assert.Equal(new [] {0, 1, 2}, callOrder);

            foreach (var mockMiddleware in mockMiddlewares)
            {
                mockMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
                mockMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
                mockMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Once);
            }
        }

        [Fact]
        public async Task Process__Given_MultipleMiddleware__When__SecondProcessFails__Then_ShortCircuit()
        {
            var environment = new Dictionary<string, object>();
            var payload = new object();

            var mockFirstMiddleware = new Mock<IMiddleware>();
            mockFirstMiddleware
                .Setup(x => x.BeforeNext(environment, payload))
                .Returns(Task.FromResult(true));

            var mockSecondMiddleware = new Mock<IMiddleware>();
            var mockThirdMiddleware = new Mock<IMiddleware>();

            var middleware = new List<Func<IMiddleware>>
            {
                () => mockFirstMiddleware.Object,
                () => mockSecondMiddleware.Object,
                () => mockThirdMiddleware.Object
            };

            var subject = new Pipeline<object>(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);

            mockFirstMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
            mockFirstMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Once);

            mockSecondMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Once);
            mockSecondMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Never);

            mockThirdMiddleware.Verify(x => x.BeforeNext(environment, payload), Times.Never);
            mockThirdMiddleware.Verify(x => x.AfterNext(environment, payload), Times.Never);
        }

        [Fact]
        public async Task Process__Given_ForkedMiddleware__Then_PassEnvironmentAndPayloadToForkedPipelines()
        {
            var payload = new object();

            IDictionary<string, object> capturedEnvironment = null;

            var leftForkMiddleware = new Mock<IMiddleware>();
            leftForkMiddleware
                .Setup(x => x.BeforeNext(It.IsAny<IDictionary<string, object>>(), payload))
                .Callback<IDictionary<string, object>, object>((e, p) => capturedEnvironment = e)
                .Returns(Task.FromResult(true));
            var leftForkMiddlewares = new List<Func<IMiddleware>> {() => leftForkMiddleware.Object};
            var leftFork = new ForkedPipeline(leftForkMiddlewares);
            
            var rightFork = new ForkedPipeline(new List<Func<IMiddleware>>());

            var tuple = new Tuple<IPipeline<object>, IPipeline<object>>(leftFork, rightFork);

            var forkedMiddleware = new TestableForkedMiddleware(tuple);

            var middleware = new List<Func<IMiddleware>> {() => forkedMiddleware};

            var subject = new Pipeline<object>(middleware);

            var result = await subject.Process(payload);
            
            Assert.True(result);

            Assert.Same(forkedMiddleware.Environment, capturedEnvironment);

            leftForkMiddleware.Verify(x => x.BeforeNext(It.IsAny<IDictionary<string, object>>(), payload), Times.Once);
            leftForkMiddleware.Verify(x => x.AfterNext(It.IsAny<IDictionary<string, object>>(), payload), Times.Once);
        }

        internal class TestableForkedMiddleware : ForkedMiddleware
        {
            public IDictionary<string, object> Environment { get; private set; }

            public TestableForkedMiddleware(Tuple<IPipeline<object>, IPipeline<object>> pipelines)
                : base(pipelines)
            {
            }

            protected override IPipeline<object> ChoosePipeline(object payload)
            {
                return LeftPipeline;
            }

            public override Task AfterNext(IDictionary<string, object> environment, object payload)
            {
                Environment = environment;
                return Task.FromResult(true);
            }
        }
    }
}
