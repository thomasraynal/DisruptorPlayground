using Disruptor;
using Disruptor.Dsl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DisruptorPlayground.Advanced2
{
    internal class ConsumerRepository : IEnumerable<IConsumerInfo>
    {
        private readonly Dictionary<object, EventProcessorInfo> _eventProcessorInfoByEventHandler;
        private readonly Dictionary<ISequence, IConsumerInfo> _eventProcessorInfoBySequence;
        private readonly List<IConsumerInfo> _consumerInfos = new List<IConsumerInfo>();

        public ConsumerRepository()
        {
            _eventProcessorInfoByEventHandler = new Dictionary<object, EventProcessorInfo>(new IdentityComparer<object>());
            _eventProcessorInfoBySequence = new Dictionary<ISequence, IConsumerInfo>(new IdentityComparer<ISequence>());
        }

        public void Add(IEventProcessor eventProcessor, object eventHandler, ISequenceBarrier sequenceBarrier)
        {
            var consumerInfo = new EventProcessorInfo(eventProcessor, eventHandler, sequenceBarrier);
            _eventProcessorInfoByEventHandler[eventHandler] = consumerInfo;
            _eventProcessorInfoBySequence[eventProcessor.Sequence] = consumerInfo;
            _consumerInfos.Add(consumerInfo);
        }


        public IEnumerator<IConsumerInfo> GetEnumerator() => _consumerInfos.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class IdentityComparer<TKey> : IEqualityComparer<TKey>
        {
            public bool Equals(TKey x, TKey y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(TKey obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
