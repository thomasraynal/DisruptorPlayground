using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DisruptorPlayground.Advanced2
{
    public class BatchEventProcessor<T> : BatchEventProcessor<T, IDataProvider<T>, ISequenceBarrier, IEventHandler<T>, BatchEventProcessor<T>.BatchStartAware>
        where T : class
    {
        public BatchEventProcessor(IDataProvider<T> dataProvider, ISequenceBarrier sequenceBarrier, IEventHandler<T> eventHandler)
            : base(dataProvider, sequenceBarrier, eventHandler, new BatchStartAware(eventHandler))
        {
        }

        public struct BatchStartAware : IBatchStartAware
        {
            private readonly IBatchStartAware _batchStartAware;

            public BatchStartAware(object eventHandler)
            {
                _batchStartAware = eventHandler as IBatchStartAware;
            }

            public void OnBatchStart(long batchSize)
            {
                if (_batchStartAware != null && batchSize != 0)
                    _batchStartAware.OnBatchStart(batchSize);
            }
        }
    }

    public class BatchEventProcessor<T, TDataProvider, TSequenceBarrier, TEventHandler, TBatchStartAware> : IBatchEventProcessor<T>
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

        // ReSharper disable FieldCanBeMadeReadOnly.Local (performance: the runtime type will be a struct)
        private TDataProvider _dataProvider;
        private TSequenceBarrier _sequenceBarrier;
        private TEventHandler _eventHandler;
        private TBatchStartAware _batchStartAware;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private readonly Sequence _sequence = new Sequence();
        private readonly ITimeoutHandler _timeoutHandler;
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim();
        private IExceptionHandler<T> _exceptionHandler = new FatalExceptionHandler();
        private volatile int _running;

        public BatchEventProcessor(TDataProvider dataProvider, TSequenceBarrier sequenceBarrier, TEventHandler eventHandler, TBatchStartAware batchStartAware)
        {
            _dataProvider = dataProvider;
            _sequenceBarrier = sequenceBarrier;
            _eventHandler = eventHandler;
            _batchStartAware = batchStartAware;

            if (eventHandler is ISequenceReportingEventHandler<T> sequenceReportingEventHandler)
                sequenceReportingEventHandler.SetSequenceCallback(_sequence);

            _timeoutHandler = eventHandler as ITimeoutHandler;
        }


        public ISequence Sequence => _sequence;

        public void Halt()
        {
            _running = RunningStates.Halted;
            _sequenceBarrier.Alert();
        }

        public bool IsRunning => _running != RunningStates.Idle;


        /// <summary>
        /// It is ok to have another thread rerun this method after a halt().
        /// </summary>
        /// <exception cref="InvalidOperationException">if this object instance is already running in a thread</exception>
        public void Run()
        {
#pragma warning disable 420
            var previousRunning = Interlocked.CompareExchange(ref _running, RunningStates.Running, RunningStates.Idle);
#pragma warning restore 420

            if (previousRunning == RunningStates.Running)
            {
                throw new InvalidOperationException("Thread is already running");
            }

            if (previousRunning == RunningStates.Idle)
            {
                _sequenceBarrier.ClearAlert();

                try
                {
                    if (_running == RunningStates.Running)
                    {
                        ProcessEvents();
                    }
                }
                finally
                {
                    _running = RunningStates.Idle;
                }
            }

        }

        private void ProcessEvents()
        {
            T evt = null;
            var nextSequence = _sequence.Value + 1L;

            while (true)
            {
                try
                {
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);

                    _batchStartAware.OnBatchStart(availableSequence - nextSequence + 1);

                    while (nextSequence <= availableSequence)
                    {
                        evt = _dataProvider[nextSequence];
                        _eventHandler.OnEvent(evt, nextSequence, nextSequence == availableSequence);
                        nextSequence++;
                    }

                    _sequence.SetValue(availableSequence);
                }
                catch (AlertException)
                {
                    if (_running != RunningStates.Running)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _exceptionHandler.HandleEventException(ex, nextSequence, evt);
                    _sequence.SetValue(nextSequence);
                    nextSequence++;
                }
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
