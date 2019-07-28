using Disruptor;
using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private long WaitUntilNext(int count)
        {
            var backoff = 1;
            var spin = new SpinWait();
            var backoffMaxYield = 8;
            var rand = new Random(Environment.TickCount & int.MaxValue);
            var upperBound = 0L;

            while (!_ringBuffer.TryNext(count, out upperBound))
            {
                for (int i = 0; i < backoff; i++)
                {
                    spin.SpinOnce();
                }

                //https://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentStack.cs,f0d50ad38c577f91
                backoff = spin.NextSpinWillYield ? rand.Next(1, backoffMaxYield) : backoff * 2;

            }

            return upperBound;
        }

        public void Publish(params Action<FxPricingEvent>[] onNextBatch)
        {

            var count = onNextBatch.Count();
            var upperBound = WaitUntilNext(count);

            var next = upperBound - (count -1);
            var current = next;

            foreach (var onNext in onNextBatch)
            {
                var ev = _ringBuffer[current++];
                onNext(ev);
            }

            _ringBuffer.Publish(next, current - 1);
        }

        public void Publish(Action<FxPricingEvent> onNext)
        {
            var next = WaitUntilNext(1);

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
