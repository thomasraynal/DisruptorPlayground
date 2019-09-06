using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced2
{
    public class EventHandlerA : IEventHandler<Event>
    {
        private readonly Random _random;

        public EventHandlerA()
        {
           _random = new Random();
        }

        public void OnEvent(Event data, long sequence, bool endOfBatch)
        {
            data.Counter++;

            _random.NextBytes(data.Data);
        }
    }
}
