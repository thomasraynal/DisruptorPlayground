using Disruptor;
using Disruptor.Dsl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced2
{
    [TestFixture]
    public class TestAdvanced2
    {
        [Test]
        public async Task ShouldBuildDisruptor()
        {
            var ringBuffer = RingBuffer<Event>.CreateSingleProducer(() => new Event(100), 16384, new BlockingWaitStrategy());

            var consumerRepository = new ConsumerRepository();

            var handlerA = new EventHandlerA();
          
            var handlers = new IEventHandler<Event>[] { handlerA };
            var barrierSequences = new ISequence[0];

            var processorSequences = new ISequence[handlers.Length];
            var barrier = ringBuffer.NewBarrier(barrierSequences);

            var executor = new BasicExecutor(TaskScheduler.Default);

            for (int i = 0; i < handlers.Length; i++)
            {
                var eventHandler = handlers[i];

                var batchEventProcessor = BatchEventProcessorFactory.Create(ringBuffer, barrier, eventHandler);

                consumerRepository.Add(batchEventProcessor, eventHandler, barrier);

                processorSequences[i] = batchEventProcessor.Sequence;

            }

            ringBuffer.AddGatingSequences(processorSequences);

            foreach (var barrierSequence in barrierSequences)
            {
                ringBuffer.RemoveGatingSequence(barrierSequence);
            }

    

            foreach (var consumerInfo in consumerRepository)
            {
                consumerInfo.Start(executor);
            }


            var next = ringBuffer.Next();
            var ev = ringBuffer[next];
            ringBuffer.Publish(next);

        
            await Task.Delay(50);

            Assert.AreEqual(1, ev.Counter);

        }

    }
}
