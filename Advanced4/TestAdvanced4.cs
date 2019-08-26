//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Running;
//using NetMQ;
//using NetMQ.Sockets;
//using Newtonsoft.Json;
//using NUnit.Framework;
//using ProtoBuf;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace DisruptorPlayground.Advanced4
//{
//    [ProtoContract]
//    public class MessageA : IMessage
//    {
//        [ProtoMember(1)]
//        public string Payload { get; set; }

//        public string GetMessage()
//        {
//            return Payload;
//        }
//    }

//    public interface IMessage
//    {
//        string GetMessage();
//    }

//    [ProtoContract]
//    public class MessageB : IMessage
//    {
//        [ProtoMember(1)]
//        public long Payload { get; set; }

//        public string GetMessage()
//        {
//            return Payload.ToString();
//        }
//    }

//    [ProtoContract]
//    public class Enveloppe
//    {
//        [ProtoMember(1)]
//        public byte[] Message { get; set; }
//        [ProtoMember(2)]
//        public Type Type { get; set; }

//    }

//    [ProtoContract]
//    public struct EnveloppeStruct
//    {
//        public EnveloppeStruct(byte[] message, Type type)
//        {
//            Message = message;
//            Type = type;
//        }

//        [ProtoMember(1)]
//        public byte[] Message { get; set; }
//        [ProtoMember(2)]
//        public Type Type { get; set; }

//    }

//    [ProtoContract]
//    public struct MessageAStruct : IMessage
//    {
//        public MessageAStruct(string payload)
//        {
//            Payload = payload;
//        }

//        [ProtoMember(1)]
//        public string Payload { get; set; }

//        public string GetMessage()
//        {
//            return Payload;
//        }
//    }

//    [ProtoContract]
//    public struct MessageBStruct : IMessage
//    {

//        public MessageBStruct(long payload)
//        {
//            Payload = payload;
//        }

//        [ProtoMember(1)]
//        public long Payload { get; set; }

//        public string GetMessage()
//        {
//            return Payload.ToString();
//        }
//    }


//    [ProtoContract]
//    public readonly struct ReadOnlyMessageBStruct : IMessage
//    {

//        public ReadOnlyMessageBStruct(long payload)
//        {
//            this.payload = payload;
//        }


//        [ProtoMember(1, IsRequired = true)]
//        private readonly long payload;

//        public long Payload
//        {
//            get { return payload; }
//        }


//        public string GetMessage()
//        {
//            return Payload.ToString();
//        }
//    }

//    public class Producer : IDisposable
//    {
//        private PushSocket _socket;

//        public Producer(string consumerUrl)
//        {
//            _socket = new PushSocket();
//            _socket.Connect(consumerUrl);
//        }

//        public void Dispose()
//        {
//            _socket.Dispose();
//        }

//        public void Send(IMessage message)
//        {
//            var enveloppe = new Enveloppe()
//            {
//                Message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)),
//                Type = message.GetType()

//            };
            
//            _socket.SendFrame(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(enveloppe)));
//        }
//    }

//    public class MessageProcessor
//    {
//        public void Process<T> (T message) where T: IMessage
//        {
//            message.GetMessage();
//        }
//    }

//    public class Consumer : IDisposable
//    {
//        private int _expected;
//        private MessageProcessor _processor;
//        private string _consumerUrl;
//        private CancellationTokenSource _cancel;
//        private Task _workProc;

//       public bool IsOk { get; set; }

//        public Consumer(string consumerUrl, int expected)
//        {
//            _expected = expected;
//            _processor = new MessageProcessor();
//            _consumerUrl = consumerUrl;
//            _cancel = new CancellationTokenSource();
//            _workProc = Task.Run(DoWork, _cancel.Token);
//        }

//        public void DoWork()
//        {
//            using (var socket = new PullSocket())
//            {
//                socket.Bind(_consumerUrl);

//                var count = 0;

//                while (!_cancel.IsCancellationRequested)
//                {
//                    var bytes = socket.ReceiveFrameBytes();
//                    var enveloppe = JsonConvert.DeserializeObject<Enveloppe>(Encoding.UTF8.GetString(bytes));
//                    var message = (IMessage)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(enveloppe.Message), enveloppe.Type);

//                    _processor.Process(message);

//                    if ((IsOk =_expected == ++count)) return;

//                }
//            }
//        }

//        public void Dispose()
//        {
//            _cancel.Cancel();
//        }
//    }

//    [TestFixture]
//    public class TestAdvanced4
//    {

//        private Random _rand;

//        public TestAdvanced4()
//        {
//            _rand = new Random();
//        }

//        public IMessage Next(bool isStruct)
//        {
//            //var isA = _rand.Next(0, 2) == 1;

//            if (isStruct)
//            {
//                return new MessageBStruct()
//                {
//                    Payload = DateTime.Now.Ticks
//                };
//            }

//            return new MessageB()
//            {
//                Payload = DateTime.Now.Ticks
//            };
//        }


//        [Params(100, 100000)]
//        public int N;

//     //   [Benchmark(Baseline = true)]
//        public void TestClass()
//        {
//            var processor = new MessageProcessor();


//            for (var i = 0; i < N; i++)
//            {

//                var message = Next(false);

//                var enveloppe = new Enveloppe()
//                {
//                    Message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)),
//                    Type = message.GetType()

//                };

//                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(enveloppe));

//                enveloppe = JsonConvert.DeserializeObject<Enveloppe>(Encoding.UTF8.GetString(bytes));
//               var messageB = (MessageB)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(enveloppe.Message), enveloppe.Type);

//                processor.Process(messageB);

//            }
//        }

//        [Benchmark(Baseline = true)]
//        public void SerializeClass()
//        {


//            for (var i = 0; i < N; i++)
//            {
//                using (var stream = new MemoryStream())
//                {
                    
//                    var message = new MessageB();

//                    Serializer.Serialize(stream, message);

//                    var bytes = stream.GetBuffer();

//                    using (var stream2 = new MemoryStream(bytes))
//                    {
//                        message = Serializer.Deserialize<MessageB>(stream2);
//                    }

//                }
//            }
//        }

//        [Test]
//        [Benchmark]
//        public void SerializeStruct()
//        {

//            for (var i = 0; i < N; i++)
//            {

//                using (var stream = new MemoryStream())
//                {

//                    var message = new MessageBStruct(10);

//                    Serializer.Serialize(stream, message);


//                    message = Serializer.Deserialize<MessageBStruct>(stream);


//                }
//            }
//        }

//        [Test]
//       // [Benchmark]
//        public void TestStruct()
//        {
//            var processor = new MessageProcessor();


//            for (var i = 0; i < N; i++)
//            {
//                var message = Next(true);

//                var enveloppe = new EnveloppeStruct(
//                    message: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)),
//                    type: message.GetType());

//                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(enveloppe));

//                enveloppe = JsonConvert.DeserializeObject<EnveloppeStruct>(Encoding.UTF8.GetString(bytes));
//                var messageB = (MessageBStruct)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(enveloppe.Message), enveloppe.Type);

//                processor.Process(messageB);

//            }
//        }

//        [Test]
//        public void Test()
//        {
//            var consumerUrl = "tcp://localhost:8080";
//            var expected = 100;

//            var consumer = new Consumer(consumerUrl, expected);


//            var producer = new Producer(consumerUrl);

//            for (var i = 0; i < expected; i++)
//            {
//                producer.Send(Next(false));
//            }

//            while (!consumer.IsOk) { }

//            consumer.Dispose();
//            producer.Dispose();
//        }

//        public static void Main(string[] args)
//        {
//            BenchmarkRunner.Run<TestAdvanced4>();
//        }


//    }
//}
