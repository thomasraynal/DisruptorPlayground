using Disruptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    public class FxPricePublisher : IDisposable
    {
        private readonly Random _random = new Random();
        private readonly FxPricingEngine _fxPricingEngine;
        private readonly CancellationTokenSource _cancel;
        private readonly bool _manualEvent;
        private Task _workProc;

        private readonly Random _rand = new Random();

        private readonly string[] _marketplaces = new[] { "fxConnect", "Harmony" };
        private readonly string[] _ccyPairs = new[] { "EUR/USD", "EUR/JPY", "EUR/GPB", "EUR/CAD" };

        public FxPricePublisher(FxPricingEngine targetEngine, bool manualEvent)
        {
            _fxPricingEngine = targetEngine;
            _cancel = new CancellationTokenSource();
            _manualEvent = manualEvent;


        }

        private void Next(FxPricingEvent fxEvent)
        {
            var mid = _rand.NextDouble() * 10;
            var spread = _rand.NextDouble() * 2;

            var ccyPair = _ccyPairs[_rand.Next(0, _ccyPairs.Count())];
            var marketplace = _marketplaces[_rand.Next(0, _marketplaces.Count())];

            fxEvent.Ask = mid + spread;
            fxEvent.Bid = mid - spread;
            fxEvent.CcyPair = ccyPair;
            fxEvent.Marketplace = marketplace;
            fxEvent.Timestamp = DateTime.Now.Ticks;
        }

        public void Start()
        {
            if (!_manualEvent)
            {
                _workProc = Task.Run(DoWork, _cancel.Token);
            }

        }

        public void PublishBatch(int count)
        {
            _fxPricingEngine.Publish(Enumerable.Range(0, _rand.Next(0, count))
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
