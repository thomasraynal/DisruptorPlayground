using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced1
{
    public class FxPricePublisher : IEventTranslator<FxPricingEvent>, IDisposable
    {
        private readonly Random _random = new Random();
        private readonly FxPricingEngine _fxPricingEngine;
        private readonly CancellationTokenSource _cancel;
        private Task _workProc;

        public FxPricePublisher(FxPricingEngine targetEngine)
        {
            _fxPricingEngine = targetEngine;
            _cancel = new CancellationTokenSource();
       
        }

        public void Run()
        {
            _workProc = Task.Run(DoWork, _cancel.Token);
        }

        public void DoWork()
        {

            while (!_cancel.IsCancellationRequested)
            {
            }
        }

        public void TranslateTo(FxPricingEvent eventData, long sequence)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _cancel.Cancel();
        }
    }
}
