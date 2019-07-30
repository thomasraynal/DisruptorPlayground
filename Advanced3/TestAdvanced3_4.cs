using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DisruptorPlayground.Advanced3
{
    public static class IdProvider
    {
        private static int _counter = 100;

        public static int Get()
        {
            return _counter++;
        }
    }

    public interface IMessage
    {
        int Id { get; }
        int Data { get; }
        bool IsProcessed { get; set; }
    }

    public class Message : IMessage
    {
        public Message(int data)
        {
            Id = IdProvider.Get();
            Data = data;
        }
        public int Id { get; }
        public int Data { get; }
        public bool IsProcessed { get; set; }
    }

    public struct MessageStruct : IMessage
    {
        public MessageStruct(int data) : this()
        {
            Id = IdProvider.Get();
            Data = data;
        }
        public int Id { get; }
        public int Data { get; }
        public bool IsProcessed { get; set; }
    }

    public class EventBusStruct
    {
        private int _counter = 0;
        public MessageStruct Next()
        {
            return new MessageStruct(_counter++);
        }
    }

    public class EventBusClassViaInterface
    {
        private int _counter = 0;
        public IMessage Next()
        {
            return new Message(_counter++);
        }
    }

    public class EventBusStructViaInterface
    {
        private int _counter = 0;
        public IMessage Next()
        {
            return new MessageStruct(_counter++);
        }
    }

    public class EventBus
    {
        private int _counter = 0;
        public Message Next()
        {
            return new Message(_counter++);
        }
    }

    public class EventProcessor
    {
        public void Process<T>(T message) where T: IMessage
        {
            message.IsProcessed = true;
        }
    }

    public class EventClassMessageProcessor
    {
        public void Process(Message message)
        {
            message.IsProcessed = true;
        }
    }

    public class EventStructMessageProcessor
    {
        public void Process(ref MessageStruct message)
        {
            message.IsProcessed = true;
        }
    }

    public interface IAdvancedEventProcessor<T> where T : IMessage
    {
        void Process(T message);
    }

    public class AdvancedEventProcessor<T> : IAdvancedEventProcessor<T> where T : IMessage
    {
        public void Process(T message)
        {
            message.IsProcessed = true;
        }
    }

    public struct AdvancedEventProcessorProxy<T> : IAdvancedEventProcessor<T> where T : IMessage
    {
        private readonly AdvancedEventProcessor<T> _proxy;

        public AdvancedEventProcessorProxy(AdvancedEventProcessor<T> proxy)
        {
            _proxy = proxy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process(T message)
        {
            _proxy.Process(message);
        }
    }

    public class AdvancedMessageConsumer<T> where T : IMessage
    {
        private IAdvancedEventProcessor<T> _processor;

        public AdvancedMessageConsumer(bool useProxy)
        {
            var processor = new AdvancedEventProcessor<T>();

            if (useProxy)
            {
                _processor = new AdvancedEventProcessorProxy<T>(processor);
            }
            else
            {
                _processor =  processor;
            }
  
        }

        public void Process(T message)
        {
            _processor.Process(message);
        }
    }

    [MemoryDiagnoser]
    public class TestAdvanced3_4_2
    {
        private Message[] _messages;

        [GlobalSetup]
        public void SetUp()
        {
            _messages = Enumerable.Range(0, 1000).Select(i => new Message(i)).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void Class()
        {
            var eventProcessor = new AdvancedMessageConsumer<Message>(false);

            foreach (var message in _messages)
            {
                eventProcessor.Process(message);

            }
        }

        [Benchmark]
        public void Proxy()
        {
            var eventProcessor = new AdvancedMessageConsumer<Message>(true);

            foreach (var message in _messages)
            {
                eventProcessor.Process(message);

            }
        }

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestAdvanced3_4_2>();
        }
    }

    [MemoryDiagnoser]
    public class TestAdvanced3_4_1
    {
        private EventBus _eventBus;
        private EventBusStruct _eventBusStruct;
        private EventBusClassViaInterface _eventBusClassViaInterface;
        private EventBusStructViaInterface _eventBusStructViaInterface;
        private EventClassMessageProcessor _eventClassMessageProcessor;
        private EventStructMessageProcessor _eventStructMessageProcessor;
        private EventProcessor _eventProcessor;

        [GlobalSetup]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _eventBusStruct = new EventBusStruct();
            _eventBusStructViaInterface = new EventBusStructViaInterface();
            _eventClassMessageProcessor = new EventClassMessageProcessor();
            _eventStructMessageProcessor = new EventStructMessageProcessor();
            _eventProcessor = new EventProcessor();

        }

        [Benchmark(Baseline = true)]
        public void GetClassAndProcessClass()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBus.Next();
                _eventClassMessageProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructAndProcessStruct()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBusStruct.Next();
                _eventStructMessageProcessor.Process(ref next);

            }
        }

        [Benchmark]
        public void GetClassAndProcessClassThroughGeneric()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBus.Next();
                _eventProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructAndProcessStructThroughGeneric()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBusStruct.Next();
                _eventProcessor.Process(next);

            }
        }


        [Benchmark]
        public void GetClassViaInterfaceAndProcessInterfaceThroughGeneric()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBusStructViaInterface.Next();
                _eventProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructViaInterfaceAndProcessInterfaceThroughGeneric()
        {
            for (var i = 0; i < 1000; i++)
            {
                var next = _eventBusStructViaInterface.Next();
                _eventProcessor.Process(next);

            }
        }



        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestAdvanced3_4_1>();
        }

    }

 
}
