using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.POC
{
    [TestFixture]
    public class TestMyRingBuffer
    {
        [Test]
        public async Task ShouldTestEventHandlerSynchronization()
        {
            var logger = new LoggerForTests();
            var ringBuffer = new MyRingBuffer(32);

            var waituntilNextEvent = 5;
            var processDuration = 5000;

            //two consumers
            var expectedEventsCount = (processDuration / waituntilNextEvent) * 2;

            var handler1 = new MyEventHandler("a", logger, ringBuffer, null, waituntilNextEvent);
            var handler2 = new MyEventHandler("b", logger, ringBuffer, handler1);
            var handler3 = new MyEventHandler("c", logger, ringBuffer, handler2);

            await Task.Delay(processDuration);

            handler1.Dispose();
            handler2.Dispose();
            handler3.Dispose();

            Assert.Greater(logger.Logs.Count(), expectedEventsCount * 0.75);
        }

    }
}
