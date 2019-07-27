using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced1
{

    public class StateHolderEventHandler : IEventHandler<FxPricingEvent>
    {
        public Dictionary<string, Marketplace> _state;

        public StateHolderEventHandler()
        {
            _state = new Dictionary<string, Marketplace>();
        }

        private void CreateMarketplaceidNotExist(FxPricingEvent @event)
        {
            var key = @event.Marketplace;

            if (!_state.ContainsKey(key))
                _state.Add(key, new Marketplace(key));
        }

        public void OnEvent(FxPricingEvent data, long sequence, bool endOfBatch)
        {
            CreateMarketplaceidNotExist(data);

            _state[data.Marketplace].Add(new FxPrice(data.CcyPair, data.Bid, data.Ask));
        }
    }
}
