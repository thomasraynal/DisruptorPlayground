using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced2
{
    public class CleanerHandler : IEventHandler<Event>
    {
        public void OnEvent(Event data, long sequence, bool endOfBatch)
        {
            data.Reset();
        }
    }
}
