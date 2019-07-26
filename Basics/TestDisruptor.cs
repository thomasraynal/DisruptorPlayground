using Disruptor;
using Disruptor.Dsl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Basics
{
    [TestFixture]
    public class TestDisruptor
    {
        [Test]
        public async Task ShouldTestDisruptor()
        {
            var disruptor = new Disruptor<Event>(() => new Event(), 32, TaskScheduler.Default, ProducerType.Single, new BusySpinWaitStrategy());

            var handlerA = new EventHandlerA();
            var handlerB = new EventHandlerB();

            disruptor.HandleEventsWith(handlerA)
                     .Then(handlerB);

            var ringBuffer = disruptor.Start();

            var publisher = new EventPublisher(500, disruptor);

            await Task.Delay(750);

            Assert.AreEqual(2, handlerA.HandledEvents.Count);
            Assert.AreEqual(2, handlerB.HandledEvents.Count);

        }

    }
}
