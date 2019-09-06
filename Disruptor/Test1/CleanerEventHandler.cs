using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class CleanerEventHandler : IEventHandler<FxPricingEvent>
    {
        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            data.CcyPair = null;
            data.Marketplace = null;
            data.Timestamp = -1;
            data.Ask = -1;
            data.Bid = -1;
 
        }
    }
}
