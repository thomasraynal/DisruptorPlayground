using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{

    public class StateHolderEventHandler : IEventHandler<FxPricingEvent>
    {
        public Dictionary<string, Marketplace> State { get; }

        public StateHolderEventHandler()
        {
            State = new Dictionary<string, Marketplace>();
        }

        private void CreateMarketplaceidNotExist(FxPricingEvent @event)
        {
            var key = @event.Marketplace;

            if (!State.ContainsKey(key))
                State.Add(key, new Marketplace(key));
        }

        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            CreateMarketplaceidNotExist(data);

            State[data.Marketplace].Add(new FxPrice(data.CcyPair, data.Bid, data.Ask));
        }
    }
}
