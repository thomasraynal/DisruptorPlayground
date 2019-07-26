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
        public volatile int Sequence = 0;
        private string _name;
        private CancellationTokenSource _cancel;
        private MyEventHandler _dependency;
        private MyRingBuffer _ringBuffer;
        private int _waitUntilNextEvent;
        private Task _workProc;

        public List<string> Logs;

        public MyEventHandler(string name, MyRingBuffer ringBuffer, MyEventHandler dependency)
        {
            _name = name;
            _cancel = new CancellationTokenSource();
            _dependency = dependency;
            _ringBuffer = ringBuffer;
            Logs = new List<string>();
            _workProc = Task.Run(DoWork, _cancel.Token);
        }


        public void DoWork()
        {
            if (_dependency == null)
            {
                var spinWait = new SpinWait();

                while (!_cancel.IsCancellationRequested)
                {
                    spinWait.SpinOnce();

                    var message = $"Create message new event {DateTime.Now.Ticks}";

                    _ringBuffer.Buffer[(Sequence) % _ringBuffer.Size] = Encoding.UTF8.GetBytes(message);

                    Sequence++;
                }

            }
            else
            {
                while (!_cancel.IsCancellationRequested)
                {
                    SpinWait.SpinUntil(() => _dependency.Sequence > Sequence);

                    Logs.Add($"{_name} do work {Sequence} {Encoding.UTF8.GetString(_ringBuffer.Buffer[Sequence % _ringBuffer.Size])}");

                    Sequence++;

                }
            }


        }

        public void Dispose()
        {
            _cancel.Cancel();
        }

    }
}
