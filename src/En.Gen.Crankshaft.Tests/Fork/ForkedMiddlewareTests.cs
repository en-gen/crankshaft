using System;
using System.Threading.Tasks;
using En.Gen.Crankshaft.Fork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace En.Gen.Crankshaft.Tests.Fork
{
    [TestClass]
    public class ForkedMiddlewareTests
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ctor__When_TupleNull__Then_ThrowArgNullEx()
        {
            new TestableForkedMiddleware(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor__When_LeftPipelineNull__Then_ThrowArgNullEx()
        {
            new TestableForkedMiddleware(Tuple.Create((IPipeline)null, Mock.Of<IPipeline>()));
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ctor__When_RightPipelineNull__Then_ThrowArgNullEx()
        {
            new TestableForkedMiddleware(Tuple.Create(Mock.Of<IPipeline>(), (IPipeline)null));
        }

        [TestMethod]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseLeft__Then_ProcessLeftAndReturnResult()
        {
            var payload = "LEFT";

            var mockLeftPipeline = new Mock<IPipeline>();
            mockLeftPipeline
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(true));

            var mockRightPipeline = new Mock<IPipeline>();

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.Process(payload);

            Assert.IsTrue(result);
            mockLeftPipeline.Verify(x => x.Process(payload), Times.Once);
            mockRightPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseRight__Then_ProcessRightAndReturnResult()
        {
            var payload = "RIGHT";

            var mockLeftPipeline = new Mock<IPipeline>();

            var mockRightPipeline = new Mock<IPipeline>();
            mockRightPipeline
                .Setup(x => x.Process(payload))
                .Returns(Task.FromResult(false));

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.Process(payload);

            Assert.IsFalse(result);
            mockLeftPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
            mockRightPipeline.Verify(x => x.Process(payload), Times.Once);
        }
        
        [TestMethod]
        public async Task Process__Given_LeftAndRightPipeline__When_ChooseNeither__Then_DoNothingAndReturnTrue()
        {
            var payload = "NOPE";

            var mockLeftPipeline = new Mock<IPipeline>();
            var mockRightPipeline = new Mock<IPipeline>();

            var subject = new TestableForkedMiddleware(Tuple.Create(mockLeftPipeline.Object, mockRightPipeline.Object));

            var result = await subject.Process(payload);

            Assert.IsTrue(result);
            mockLeftPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
            mockRightPipeline.Verify(x => x.Process(It.IsAny<object>()), Times.Never);
        }

        internal class TestableForkedMiddleware : ForkedMiddleware
        {
            public TestableForkedMiddleware(Tuple<IPipeline, IPipeline> pipelines) :
                base(pipelines)
            {
            }

            protected override IPipeline ChoosePipeline(object payload)
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

            public override Task PostProcess(object payload)
            {
                throw new NotImplementedException();
            }
        }
    }
}
