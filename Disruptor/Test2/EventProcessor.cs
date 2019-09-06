using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DisruptorPlayground.Advanced2
{

    public class EventProcessor<T> : EventProcessor<T, IDataProvider<T>, ISequenceBarrier, IEventHandler<T>, BatchEventProcessor<T>.BatchStartAware>
    where T : class
    {
        public EventProcessor(IDataProvider<T> dataProvider, ISequenceBarrier sequenceBarrier, IEventHandler<T> eventHandler)
            : base(dataProvider, sequenceBarrier, eventHandler)
        {
        }
    }


    public class EventProcessor<T, TDataProvider, TSequenceBarrier, TEventHandler, TBatchStartAware> : IBatchEventProcessor<T>
        where T : class

        where TDataProvider : IDataProvider<T>
        where TSequenceBarrier : ISequenceBarrier
        where TEventHandler : IEventHandler<T>
        where TBatchStartAware : IBatchStartAware
    {
        private static class RunningStates
        {
            public const int Idle = 0;
            public const int Halted = Idle + 1;
            public const int Running = Halted + 1;
        }

        private TDataProvider _dataProvider;
        private TSequenceBarrier _sequenceBarrier;
        private TEventHandler _eventHandler;

        private readonly Sequence _sequence = new Sequence();
        private volatile int _running;

        public EventProcessor(TDataProvider dataProvider, TSequenceBarrier sequenceBarrier, TEventHandler eventHandler)
        {
            _dataProvider = dataProvider;
            _sequenceBarrier = sequenceBarrier;
            _eventHandler = eventHandler;
        }


        public ISequence Sequence => _sequence;

        public void Halt()
        {
            _running = RunningStates.Halted;
        }

        public bool IsRunning => _running != RunningStates.Idle;

        public void Run()
        {
            _running = RunningStates.Running;

            ProcessEvents();
        }

        private void ProcessEvents()
        {
            T evt = null;
            var nextSequence = _sequence.Value + 1L;

            while (true)
            {

                var availableSequence = _sequenceBarrier.WaitFor(nextSequence);

                while (nextSequence <= availableSequence)
                {
                    evt = _dataProvider[nextSequence];
                    _eventHandler.OnEvent(evt, nextSequence, nextSequence == availableSequence);
                    nextSequence++;
                }

                _sequence.SetValue(availableSequence);

            }
        }

        public void SetExceptionHandler(IExceptionHandler<T> exceptionHandler)
        {
            throw new NotImplementedException();
        }

        public void WaitUntilStarted(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
