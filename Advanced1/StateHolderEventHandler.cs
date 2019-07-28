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

        private Marketplace GetOrCreateMarketplace(string marketplace)
        {

            if (!State.ContainsKey(marketplace))
                State.Add(marketplace, new Marketplace(marketplace));

            return State[marketplace];
        }

        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            var marketplace = GetOrCreateMarketplace(data.Marketplace);

            marketplace.Add(new FxPrice(data.CcyPair, data.Bid, data.Ask));
        }
    }
}
