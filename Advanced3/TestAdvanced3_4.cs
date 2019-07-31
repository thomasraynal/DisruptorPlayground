using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using System;
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

    public static class Helper
    {
        public static readonly Random Rand = new Random();

        public static byte[] RandomBytes()
        {
            var bytes = new byte[10];
             Rand.NextBytes(bytes);
            return bytes;
        }
        public static string RandomString(int length)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

    }

    public interface IMessage
    {
        int Id { get; }
        byte[] Data { get; }
        bool IsProcessed { get; set; }
    }

    public class Message : IMessage
    {
        public Message(int data)
        {
            Id = IdProvider.Get();
            Data = Helper.RandomBytes();
        }
        public int Id { get; }
        public byte[] Data { get; set; }
        public bool IsProcessed { get; set; }
    }

    public struct MessageStruct : IMessage
    {
        public MessageStruct(int data)
        {
            Id = IdProvider.Get();
            Data = Helper.RandomBytes();
            IsProcessed = false;
            Test = 0;
        }
        //int =4
        public int Id { get; }
        //int =8
        public byte[] Data { get; set; }

        //4
        public bool IsProcessed { get; set; }

        public int Test { get; }
    }

    public class EventBusStruct
    {
        private int _counter = 0;
        private MessageStruct[] _items;

        public EventBusStruct(MessageStruct[] items)
        {
            _items = items;
        }

        public MessageStruct Next()
        {
            return _items[_counter++];
        }
    }


    public class EventBusClassViaInterface
    {
        private int _counter = 0;
        private Message[] _items;

        public EventBusClassViaInterface(Message[] items)
        {
            _items = items;
        }

        public IMessage Next()
        {
            return _items[_counter++];
        }
    }

    public class EventBusStructViaInterface
    {
        private int _counter = 0;
        private MessageStruct[] _items;

        public EventBusStructViaInterface(MessageStruct[] items)
        {
            _items = items;
        }

        public IMessage Next()
        {
            return _items[_counter++];
        }
    }

    public class EventBus
    {
        private int _counter = 0;

        private Message[] _items;

        public EventBus(Message[] items)
        {
            _items = items;
        }

        public Message Next()
        {
            return _items[_counter++];
        }
    }

    public class EventProcessor
    {
        public void Process<T>(T message) where T: IMessage
        {
            message.IsProcessed = true;
        }
    }

    public class EventInterfaceProcessor
    {
        public void Process(IMessage message)
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

    //public interface IAdvancedEventProcessor<T> where T : IMessage
    //{
    //    void Process(T message);
    //}

    //public class AdvancedEventProcessor<T> : IAdvancedEventProcessor<T> where T : IMessage
    //{
    //    public void Process(T message)
    //    {
    //        message.IsProcessed = true;
    //    }
    //}

    //public struct AdvancedEventProcessorProxy<T> : IAdvancedEventProcessor<T> where T : IMessage
    //{
    //    private readonly AdvancedEventProcessor<T> _proxy;

    //    public AdvancedEventProcessorProxy(AdvancedEventProcessor<T> proxy)
    //    {
    //        _proxy = proxy;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Process(T message)
    //    {
    //        _proxy.Process(message);
    //    }
    //}

    //public class AdvancedMessageConsumer<T> where T : IMessage
    //{
    //    private IAdvancedEventProcessor<T> _processor;

    //    public AdvancedMessageConsumer(bool useProxy)
    //    {
    //        var processor = new AdvancedEventProcessor<T>();

    //        if (useProxy)
    //        {
    //            _processor = new AdvancedEventProcessorProxy<T>(processor);
    //        }
    //        else
    //        {
    //            _processor =  processor;
    //        }
  
    //    }

    //    public void Process(T message)
    //    {
    //        _processor.Process(message);
    //    }
    //}

    //[MemoryDiagnoser]
    //public class TestAdvanced3_4_2
    //{
    //    private Message[] _messages;

    //    [GlobalSetup]
    //    public void SetUp()
    //    {
    //        _messages = Enumerable.Range(0, 1000).Select(i => new Message(i)).ToArray();
    //    }

    //    [Benchmark(Baseline = true)]
    //    public void Class()
    //    {
    //        var eventProcessor = new AdvancedMessageConsumer<Message>(false);

    //        foreach (var message in _messages)
    //        {
    //            eventProcessor.Process(message);

    //        }
    //    }

    //    [Benchmark]
    //    public void Proxy()
    //    {
    //        var eventProcessor = new AdvancedMessageConsumer<Message>(true);

    //        foreach (var message in _messages)
    //        {
    //            eventProcessor.Process(message);

    //        }
    //    }

    //    public static void Main(string[] args)
    //    {
    //        BenchmarkRunner.Run<TestAdvanced3_4_2>();
    //    }
    //}

    [MemoryDiagnoser]
    public class TestAdvanced3_4_1
    {
        private EventBus _eventBus;
        private EventBusStruct _eventBusStruct;
        private EventBusClassViaInterface _eventBusClassViaInterface;
        private EventBusStructViaInterface _eventBusStructViaInterface;
        private EventInterfaceProcessor _eventInterfaceProcessor;
        private EventClassMessageProcessor _eventClassMessageProcessor;
        private EventStructMessageProcessor _eventStructMessageProcessor;
        private EventProcessor _eventProcessor;
        private MessageStruct[] _structs;
        private Message[] _classes;


        [IterationSetup]
        public void SetUp()
        {

            _structs = Enumerable.Range(0, 100000).Select(i => new MessageStruct(i)).ToArray();
            _classes = Enumerable.Range(0, 100000).Select(i => new Message(i)).ToArray();

            _eventBus = new EventBus(_classes);
            _eventBusStruct = new EventBusStruct(_structs);
            _eventBusClassViaInterface = new EventBusClassViaInterface(_classes);
            _eventBusStructViaInterface = new EventBusStructViaInterface(_structs);

            _eventInterfaceProcessor = new EventInterfaceProcessor();
            _eventClassMessageProcessor = new EventClassMessageProcessor();
            _eventStructMessageProcessor = new EventStructMessageProcessor();
            _eventProcessor = new EventProcessor();

        }

        [Params(100, 100000)]
        public int N;


        [Benchmark(Baseline = true)]
        public void GetClassAndProcessClass()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBus.Next();
                _eventClassMessageProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructAndProcessStruct()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusStruct.Next();
                _eventStructMessageProcessor.Process(ref next);

            }
        }

        [Benchmark]
        public void GetClassAndProcessClassThroughGenerics()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBus.Next();
                _eventProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructAndProcessStructThroughGenerics()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusStruct.Next();
                _eventProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetClassViaInterfaceAndProcessInterfaceThroughGenerics()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusClassViaInterface.Next();
                _eventProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructViaInterfaceAndProcessInterfaceThroughGenerics()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusStructViaInterface.Next();
                _eventProcessor.Process(next);

            }
        }


        [Benchmark]
        public void GetClassViaInterfaceAndProcessInterface()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusClassViaInterface.Next();
                _eventInterfaceProcessor.Process(next);

            }
        }

        [Benchmark]
        public void GetStructViaInterfaceAndProcessInterface()
        {
            for (var i = 0; i < N; i++)
            {
                var next = _eventBusStructViaInterface.Next();
                _eventInterfaceProcessor.Process(next);

            }
        }

        [Benchmark]
        public void CreateAndProcessClasses()
        {
  
            for (var i = 0; i < N; i++)
            {
                var message = new Message(i);
                _eventInterfaceProcessor.Process(message);

            }
        }

        [Benchmark]
        public void CreateAndProcessStructs()
        {

            for (var i = 0; i < N; i++)
            {
                var message = new MessageStruct(i);
                _eventInterfaceProcessor.Process(message);

            }
        }


        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestAdvanced3_4_1>();
        }

    }

 
}
