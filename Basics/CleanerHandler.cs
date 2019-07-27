using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Basics
{
    public class CleanerHandler : IEventHandler<Event>
    {
        public void OnEvent(Event data, long sequence, bool endOfBatch)
        {
            data.Data = null;
        }
    }
}
