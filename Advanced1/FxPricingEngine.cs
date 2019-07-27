using Disruptor;
using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    public class FxPricingEngine : IDisposable
    {
        private readonly Disruptor<FxPricingEvent> _disruptor;
        private readonly RingBuffer<FxPricingEvent> _ringBuffer;

        public FxPricingEngine(params IEventHandler<FxPricingEvent>[] handlers)
        {
            _disruptor = new Disruptor<FxPricingEvent>(() => new FxPricingEvent(), 16384, TaskScheduler.Default, ProducerType.Single, new BusySpinWaitStrategy());


            EventHandlerGroup<FxPricingEvent> group = null;

            foreach (var handler in handlers)
            {
                if (null == group)
                {
                    group = _disruptor.HandleEventsWith(handler);
                }
                else
                {
                    group = group.Then(handler);
                }
            }

            _ringBuffer = _disruptor.RingBuffer;
        }

        public void Publish(params Action<FxPricingEvent>[] onNextBatch)
        {
            
            var next = _ringBuffer.Next(onNextBatch.Count());
            var current = next;

            foreach(var onNext in onNextBatch)
            {
                var ev = _ringBuffer[current++];
                onNext(ev);
            }

            _ringBuffer.Publish(next, current - 1);
        }

        public void Publish(Action<FxPricingEvent> onNext)
        {
            var next = _ringBuffer.Next();
            var ev = _ringBuffer[next];

            onNext(ev);

            _ringBuffer.Publish(next);
        }

        public void Dispose()
        {
            _disruptor.Shutdown();
        }

        public void Start()
        {
            _disruptor.Start();
        }
   
    }
}
