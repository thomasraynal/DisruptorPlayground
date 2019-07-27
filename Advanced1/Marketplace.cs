using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisruptorPlayground.Advanced1
{
    public class Marketplace
    {
        private readonly Dictionary<string, List<FxPrice>> _priceHistory;
        public string Name { get; }

        public FxPrice GetCurrentPrice (string assetName)
        {
            if (!_priceHistory.ContainsKey(assetName)) return null;

            return _priceHistory[assetName].LastOrDefault();
        }

        private List<FxPrice> GetOrCreatePriceList(string asset)
        {
            if (!_priceHistory.ContainsKey(asset))
            {
                _priceHistory.Add(asset, new List<FxPrice>());
            }

            return _priceHistory[asset];
        }

        public void Add(FxPrice price)
        {
            var prices = GetOrCreatePriceList(price.CcyPair);
            prices.Add(price);
        }

        public Marketplace(string name)
        {
            Name = name;
            _priceHistory = new Dictionary<string, List<FxPrice>>();
        }
    }
}
