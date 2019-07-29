using Disruptor;
using Disruptor.Dsl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced2
{
    [TestFixture]
    public class TestAdvanced2
    {

        [Test]
        public async Task ShouldUseRingBuffer()
        {
            var ringBuffer = RingBuffer<Event>.CreateSingleProducer(() => new Event(100), 16384, new BlockingWaitStrategy());

            var handlerA = new EventHandlerA();
            var handlerABarrier = ringBuffer.NewBarrier(new ISequence[0]);
            var handlerASequence = new Sequence();

            var handlerB = new CleanerHandler();
            var handlerBBarrier = ringBuffer.NewBarrier(new ISequence[] { handlerASequence });

            var next = ringBuffer.Next();
            var ev = ringBuffer[next];

            Task.Run(() =>
            {
                var sequence = handlerABarrier.WaitFor(next);
              
                handlerA.OnEvent(ev, sequence, true);
                handlerASequence.SetValue(sequence);

                sequence = handlerBBarrier.WaitFor(next);
                handlerB.OnEvent(ev, sequence, true);

            });

           
          
            ringBuffer.Publish(next);


            await Task.Delay(50);

            Assert.AreEqual(2, ev.Counter);

            Assert.AreEqual(0, ev.Data.Sum(i => i));

        }

        [Test]
        public async Task ShouldBuildDisruptor()
        {
            var ringBuffer = RingBuffer<Event>.CreateSingleProducer(() => new Event(100), 16384, new BlockingWaitStrategy());
            var executor = new BasicExecutor(TaskScheduler.Default);
            var consumerRepository = new ConsumerRepository();

            var handlerA = new EventHandlerA();
            var handlerABarrier = ringBuffer.NewBarrier(new ISequence[0]);
            var handlerAEventProcessor = new EventProcessor<Event>(ringBuffer, handlerABarrier, handlerA);

            consumerRepository.Add(handlerAEventProcessor, handlerA, handlerABarrier);

            var handlerB = new CleanerHandler();
            var handlerBBarrier = ringBuffer.NewBarrier(new ISequence[] { handlerAEventProcessor.Sequence });
            var handlerBEventProcessor = new EventProcessor<Event>(ringBuffer, handlerBBarrier, handlerB);

            consumerRepository.Add(handlerBEventProcessor, handlerB, handlerBBarrier);

            foreach (var consumerInfo in consumerRepository)
            {
                consumerInfo.Start(executor);
            }

            var next = ringBuffer.Next();
            var ev = ringBuffer[next];
            ringBuffer.Publish(next);

            await Task.Delay(50);

            Assert.AreEqual(2, ev.Counter);

            Assert.AreEqual(0, ev.Data.Sum(i => i));

        }

    }

 
}
