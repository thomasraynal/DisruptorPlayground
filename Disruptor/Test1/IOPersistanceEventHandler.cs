using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DisruptorPlayground.Advanced1
{
   
    public class IOPersistanceEventHandler : IEventHandler<FxPricingEvent>
    {
        private readonly StringBuilder _currentBatch;

        public int WriteCount { get; private set; }

        public IOPersistanceEventHandler()
        {
            _currentBatch = new StringBuilder();
        }

        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            _currentBatch.Append($"{data.Ask};{data.Bid};{data.CcyPair};{data.Marketplace};{data.Timestamp}");

            WriteCount++;

            if (endOfBatch)
            {
                Thread.Sleep(10);
                _currentBatch.Clear();
            }
        }
    }
}
