using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var subject = new Pipeline(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessGeneric__Given_NoMiddleware__Then_DoNotProcess()
        {
            var payload = new object();

            var middleware = new List<Func<IMiddleware>>();

            var subject = new Pipeline<string>(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__Then_ProcessPayload()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> {() => mockMiddleware.Object};

            var subject = new Pipeline(middleware);
            var result = await subject.Process(payload);
            
            Assert.True(result);
            mockMiddleware.Verify(x => x.Process(payload), Times.Once);
        }

        [Fact]
        public async Task ProcessGeneric__Given_SingleMiddleware__Then_ProcessPayload()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline<string>(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
            mockMiddleware.Verify(x => x.Process(payload), Times.Once);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__When_ProcessSuccess__Then_PostProcessPayload()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
            mockMiddleware.Verify(x => x.PostProcess(payload), Times.Once);
        }

        [Fact]
        public async Task ProcessGeneric__Given_SingleMiddleware__When_ProcessSuccess__Then_PostProcessPayload()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline<string>(middleware);
            var result = await subject.Process(payload);

            Assert.True(result);
            mockMiddleware.Verify(x => x.PostProcess(payload), Times.Once);
        }

        [Fact]
        public async Task Process__Given_SingleMiddleware__When_ProcessFail__Then_DoNotPostProcess()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(false));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);
            mockMiddleware.Verify(x => x.PostProcess(payload), Times.Never);
        }

        [Fact]
        public async Task ProcessGeneric__Given_SingleMiddleware__When_ProcessFail__Then_DoNotPostProcess()
        {
            var payload = new object();

            var mockMiddleware = new Mock<IMiddleware>();
            mockMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(false));

            var middleware = new List<Func<IMiddleware>> { () => mockMiddleware.Object };

            var subject = new Pipeline<string>(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);
            mockMiddleware.Verify(x => x.PostProcess(payload), Times.Never);
        }

        [Fact]
        public async Task Process__Given_MultipleMiddleware__When__AllSuccess__Then_AllMiddlewareProcessPayload_And_AllMiddlewarePostProcessPayload()
        {
            var payload = new object();
            
            var callOrder = new List<int>();

            var mockMiddlewares = Enumerable.Range(0, 3)
                .Select(x =>
                {
                    var mockMiddleware = new Mock<IMiddleware>();
                    mockMiddleware
                        .Setup(mock => mock.Process(payload))
                        .Returns(Task.FromResult(true))
                        .Callback(() => callOrder.Add(x));
                    return mockMiddleware;
                })
                .ToArray();

            var middlewares = mockMiddlewares
                .Select<Mock<IMiddleware>, Func<IMiddleware>>(x => () => x.Object)
                .ToList();

            var subject = new Pipeline(middlewares);
            var result = await subject.Process(payload);
            
            Assert.True(result);
            Assert.Equal(new [] {0, 1, 2}, callOrder);

            foreach (var mockMiddleware in mockMiddlewares)
            {
                mockMiddleware.Verify(x => x.Process(payload), Times.Once);
                mockMiddleware.Verify(x => x.Process(payload), Times.Once);
                mockMiddleware.Verify(x => x.PostProcess(payload), Times.Once);
            }
        }

        [Fact]
        public async Task ProcessGeneric__Given_MultipleMiddleware__When__AllSuccess__Then_AllMiddlewareProcessPayload_And_AllMiddlewarePostProcessPayload()
        {
            var payload = new object();

            var callOrder = new List<int>();

            var mockMiddlewares = Enumerable.Range(0, 3)
                .Select(x =>
                {
                    var mockMiddleware = new Mock<IMiddleware>();
                    mockMiddleware
                        .Setup(mock => mock.Process(payload))
                        .Returns(Task.FromResult(true))
                        .Callback(() => callOrder.Add(x));
                    return mockMiddleware;
                })
                .ToArray();

            var middlewares = mockMiddlewares
                .Select<Mock<IMiddleware>, Func<IMiddleware>>(x => () => x.Object)
                .ToList();

            var subject = new Pipeline<string>(middlewares);
            var result = await subject.Process(payload);

            Assert.True(result);
            Assert.Equal(new[] { 0, 1, 2 }, callOrder);

            foreach (var mockMiddleware in mockMiddlewares)
            {
                mockMiddleware.Verify(x => x.Process(payload), Times.Once);
                mockMiddleware.Verify(x => x.Process(payload), Times.Once);
                mockMiddleware.Verify(x => x.PostProcess(payload), Times.Once);
            }
        }

        [Fact]
        public async Task Process__Given_MultipleMiddleware__When__SecondProcessFails__Then_ShortCircuit()
        {
            var payload = new object();

            var mockFirstMiddleware = new Mock<IMiddleware>();
            mockFirstMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var mockSecondMiddleware = new Mock<IMiddleware>();
            var mockThirdMiddleware = new Mock<IMiddleware>();

            var middleware = new List<Func<IMiddleware>>
            {
                () => mockFirstMiddleware.Object,
                () => mockSecondMiddleware.Object,
                () => mockThirdMiddleware.Object
            };

            var subject = new Pipeline(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);

            mockFirstMiddleware.Verify(x => x.Process(payload), Times.Once);
            mockFirstMiddleware.Verify(x => x.PostProcess(payload), Times.Once);

            mockSecondMiddleware.Verify(x => x.Process(payload), Times.Once);
            mockSecondMiddleware.Verify(x => x.PostProcess(payload), Times.Never);

            mockThirdMiddleware.Verify(x => x.Process(payload), Times.Never);
            mockThirdMiddleware.Verify(x => x.PostProcess(payload), Times.Never);
        }

        [Fact]
        public async Task ProcessGeneric__Given_MultipleMiddleware__When__SecondProcessFails__Then_ShortCircuit()
        {
            var payload = new object();

            var mockFirstMiddleware = new Mock<IMiddleware>();
            mockFirstMiddleware
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var mockSecondMiddleware = new Mock<IMiddleware>();
            var mockThirdMiddleware = new Mock<IMiddleware>();

            var middleware = new List<Func<IMiddleware>>
            {
                () => mockFirstMiddleware.Object,
                () => mockSecondMiddleware.Object,
                () => mockThirdMiddleware.Object
            };

            var subject = new Pipeline<string>(middleware);
            var result = await subject.Process(payload);

            Assert.False(result);

            mockFirstMiddleware.Verify(x => x.Process(payload), Times.Once);
            mockFirstMiddleware.Verify(x => x.PostProcess(payload), Times.Once);

            mockSecondMiddleware.Verify(x => x.Process(payload), Times.Once);
            mockSecondMiddleware.Verify(x => x.PostProcess(payload), Times.Never);

            mockThirdMiddleware.Verify(x => x.Process(payload), Times.Never);
            mockThirdMiddleware.Verify(x => x.PostProcess(payload), Times.Never);
        }
    }
}
