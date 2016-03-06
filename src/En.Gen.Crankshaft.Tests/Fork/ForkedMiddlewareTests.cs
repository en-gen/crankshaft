using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using En.Gen.Crankshaft.Fork;
using Moq;
using Xunit;

namespace En.Gen.Crankshaft.Tests.Fork
{
    public class ForkedMiddlewareTests
    {
        [Fact]
        public void ctor__When_TupleNull__Then_ThrowArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() => new TestableForkedMiddleware(null));
        }

        [Fact]
        public void ctor__When_LeftPipelineNull__Then_ThrowArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() => new TestableForkedMiddleware(Tuple.Create((IPipeline<object>)null, Mock.Of<IPipeline<object>>())));
        }

        [Fact]
        public void ctor__When_RightPipelineNull__Then_ThrowArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() => new TestableForkedMiddleware(new Tuple<IPipeline<object>, IPipeline<object>>(Mock.Of<IPipeline<object>>(), (IPipeline<object>)null)));
        }

        [Fact]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseLeft__Then_ProcessLeftAndReturnResult()
        {
            var payload = "LEFT";

            var mockLeftPipeline = new Mock<IPipeline<object>>();
            mockLeftPipeline
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var mockRightPipeline = new Mock<IPipeline<object>>();

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.BeforeNext(new Dictionary<string, object>(), payload);

            Assert.True(result);
            mockLeftPipeline.Verify(x => x.Process(payload), Times.Once);
            mockRightPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseRight__Then_ProcessRightAndReturnResult()
        {
            var payload = "RIGHT";

            var mockLeftPipeline = new Mock<IPipeline<object>>();

            var mockRightPipeline = new Mock<IPipeline<object>>();
            mockRightPipeline
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(false));

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.BeforeNext(new Dictionary<string, object>(), payload);

            Assert.False(result);
            mockLeftPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
            mockRightPipeline.Verify(x => x.Process(payload), Times.Once);
        }
        
        [Fact]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseNeither__Then_DoNothingAndReturnTrue()
        {
            var payload = "NOPE";

            var mockLeftPipeline = new Mock<IPipeline<object>>();
            var mockRightPipeline = new Mock<IPipeline<object>>();

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.BeforeNext(new Dictionary<string, object>(), payload);

            Assert.True(result);
            mockLeftPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
            mockRightPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
        }

        internal class TestableForkedMiddleware : ForkedMiddleware
        {
            public TestableForkedMiddleware(Tuple<IPipeline<object>, IPipeline<object>> pipelines) :
                base(pipelines)
            {
            }

            protected override IPipeline<object> ChoosePipeline(object payload)
            {
                var payloadString = payload as string;
                switch (payloadString)
                {
                    case "LEFT":
                        {
                            return LeftPipeline;
                        }
                    case "RIGHT":
                        {
                            return RightPipeline;
                        }
                    default:
                        {
                            return null;
                        }
                }
            }

            public override Task AfterNext(IDictionary<string, object> environment, object payload)
            {
                throw new NotImplementedException();
            }
        }
    }
}
