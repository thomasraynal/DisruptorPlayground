using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorPlayground.POC
{
    public class MyEventHandler : IDisposable
    {
        public volatile int Sequence = -1;
        private string _name;
        private CancellationTokenSource _cancel;
        private MyEventHandler _dependency;
        private MyRingBuffer _ringBuffer;
        private int _waitUntilNextEvent;
        private Task _workProc;
        private ILogger _logger;

        public MyEventHandler(string name, ILogger logger, MyRingBuffer ringBuffer, MyEventHandler dependency, int waitUntilNextEvent = 0)
        {
            _name = name;
            _cancel = new CancellationTokenSource();
            _dependency = dependency;
            _ringBuffer = ringBuffer;
            _waitUntilNextEvent = waitUntilNextEvent;
            _workProc = Task.Run(DoWork, _cancel.Token);
            _logger = logger;
        }


        public void DoWork()
        {
            if (_dependency == null)
            {
            
                while (!_cancel.IsCancellationRequested)
                {

                    Thread.Sleep(_waitUntilNextEvent);

                    var message = $"Create message new event {DateTime.Now.Ticks}";

                    _ringBuffer.Buffer[(Sequence + 1) % _ringBuffer.Size] = Encoding.UTF8.GetBytes(message);

                    Sequence++;
                }

            }
            else
            {
                while (!_cancel.IsCancellationRequested)
                {
                    SpinWait.SpinUntil(() => _dependency.Sequence > Sequence);

                    _logger.LogInformation($"{_name} do work {++Sequence} {Encoding.UTF8.GetString(_ringBuffer.Buffer[Sequence % _ringBuffer.Size])}");

                }
            }


        }

        public void Dispose()
        {
            _cancel.Cancel();
        }

    }
}
