using Disruptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    public class Cache
    {
        public string CcyPair;
        public double Bid;
        public double Ask;
        public string Marketplace;
    }

    public class FxPricePublisher : IDisposable
    {
 

        private readonly Random _random = new Random();
        private readonly FxPricingEngine _fxPricingEngine;
        private readonly CancellationTokenSource _cancel;

        private Task _workProc;

        private readonly Random _rand = new Random();

        private readonly string[] _marketplaces = new[] { "fxConnect", "Harmony" };
        private readonly string[] _ccyPairs = new[] { "EUR/USD", "EUR/JPY", "EUR/GPB", "EUR/CAD" };

        public List<Cache> Cache { get; }

        public FxPricePublisher(FxPricingEngine targetEngine)
        {
            _fxPricingEngine = targetEngine;
            _cancel = new CancellationTokenSource();
     

            Cache = new List<Cache>();

        }

        private void Next(FxPricingEvent fxEvent)
        {
            var mid = _rand.NextDouble() * 10;
            var spread = _rand.NextDouble() * 2;

            var ccyPair = _ccyPairs[_rand.Next(0, _ccyPairs.Count())];
            var marketplace = _marketplaces[_rand.Next(0, _marketplaces.Count())];

            var cache = new Cache()
            {
                Ask = mid + spread,
                Bid = mid - spread,
                CcyPair = ccyPair,
                Marketplace = marketplace
            };

            Cache.Add(cache);

            fxEvent.Ask = cache.Ask;
            fxEvent.Bid = cache.Bid;
            fxEvent.CcyPair = cache.CcyPair;
            fxEvent.Marketplace = cache.Marketplace;
            fxEvent.Timestamp = DateTime.Now.Ticks;
        }

        public void StartGenerateEvents()
        {
            _workProc = Task.Run(DoWork, _cancel.Token);
        }

        public void PublishBatch(int count)
        {
            _fxPricingEngine.Publish(Enumerable.Range(0, count)
                                            .Select(_ => new Action<FxPricingEvent>((ev) => Next(ev)))
                                            .ToArray());
        }

        public void PublishOne()
        {
            _fxPricingEngine.Publish((ev) => Next(ev));
        }

        public void DoWork()
        {

            while (!_cancel.IsCancellationRequested)
            {

                if (_rand.Next(0, 2) == 1)
                {

                    PublishOne();
                }
                else
                {
                    PublishBatch(_rand.Next(10, 21));
                }
            }
        }

        public void Dispose()
        {
            _cancel.Cancel();
        }
    }
}
