using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class ReplicatorEventHandler : IEventHandler<FxPricingEvent>
    {
        private FxPricingEngine _replica;

        public ReplicatorEventHandler(FxPricingEngine replica)
        {
            _replica = replica;
        }

        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            _replica.Publish((ev) =>
            {
                ev.Ask = data.Ask;
                ev.Bid = data.Bid;
                ev.CcyPair = data.CcyPair;
                ev.Marketplace = data.Marketplace;
                ev.Timestamp = data.Timestamp;
            });
        }
    }
}
