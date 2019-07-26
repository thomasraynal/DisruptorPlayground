using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Basics
{
    public class EventHandlerA : IEventHandler<Event>, ILifecycleAware
    {
        public EventHandlerA()
        {
            HandledEvents = new List<string>();
        }

        public List<string> HandledEvents { get; }

        public void OnEvent(Event data, long sequence, bool endOfBatch)
        {
            HandledEvents.Add(Encoding.ASCII.GetString(data.Data));
        }

        public void OnShutdown()
        {

        }

        public void OnStart()
        {

        }
    }
}
