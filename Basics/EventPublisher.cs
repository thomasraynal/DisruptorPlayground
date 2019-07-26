using Disruptor;
using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorPlayground.Basics
{
    public class EventPublisher : IEventTranslator<Event>, IDisposable
    {
        private CancellationTokenSource _cancel;
        private Disruptor<Event> _disruptor;
        private readonly int _averageFrequency;
        private readonly Task _workProc;

        private string _currentMessage;

        //https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        private readonly Random _rand = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_rand.Next(s.Length)]).ToArray());
        }

        public EventPublisher(int averageFrequency, Disruptor<Event> disruptor)
        {
            _cancel = new CancellationTokenSource();
            _disruptor = disruptor;
            _averageFrequency = averageFrequency;
            _workProc = Task.Run(DoWork, _cancel.Token);
        }

        public void Dispose()
        {
            _cancel.Cancel();
        }

        public void DoWork()
        {

            while (!_cancel.IsCancellationRequested)
            {
                var bound = (int)(_averageFrequency * 0.1);

                _currentMessage = RandomString(10);

                _disruptor.PublishEvent(this);

                Thread.Sleep(_rand.Next(_averageFrequency - bound, _averageFrequency + bound));
            }
        }

        public void TranslateTo(Event eventData, long sequence)
        {
            eventData.Data = Encoding.ASCII.GetBytes(_currentMessage);
        }
    }
}
