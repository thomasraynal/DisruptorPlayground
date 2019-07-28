using Disruptor;
using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced2
{
    internal class EventProcessorInfo : IConsumerInfo
    {
        public EventProcessorInfo(IEventProcessor eventProcessor, object eventHandler, ISequenceBarrier barrier)
        {
            EventProcessor = eventProcessor;
            Handler = eventHandler;
            Barrier = barrier;
            IsEndOfChain = true;
        }

        public IEventProcessor EventProcessor { get; }

        public ISequence[] Sequences => new[] { EventProcessor.Sequence };

        public object Handler { get; }

        public ISequenceBarrier Barrier { get; }

        public bool IsEndOfChain { get; private set; }

        public void Start(IExecutor executor)
        {
            executor.Execute(EventProcessor.Run);
        }

        public void Halt()
        {
            EventProcessor.Halt();
        }

        public void MarkAsUsedInBarrier()
        {
            IsEndOfChain = false;
        }

        public bool IsRunning => EventProcessor.IsRunning;
    }
}
