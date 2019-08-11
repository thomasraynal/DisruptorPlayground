using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced4
{
    public readonly struct DispatchStruct
    {
        public DispatchStruct(Action<IMessage> action, IMessage message)
        {
            Message = message;
            Action = action;
        }

        public readonly IMessage Message;
        public readonly Action<IMessage> Action;
    }

    public class DispatchClass
    {
        public DispatchClass(Action<IMessage> action, IMessage message)
        {
            Message = message;
            Action = action;
        }

        public readonly IMessage Message;
        public readonly Action<IMessage> Action;
    }

    [TestFixture]
    [MemoryDiagnoser]
    public class TestAdvanced4_2
    {

        public IMessage Next(bool isStruct)
        {
            //var isA = _rand.Next(0, 2) == 1;

            if (isStruct)
            {
                return new MessageBStruct()
                {
                    Payload = DateTime.Now.Ticks
                };
            }

            return new MessageB()
            {
                Payload = DateTime.Now.Ticks
            };
        }


        [Params(100, 100000)]
        public int N = 10;

        [Test]
        [Benchmark]
        public void TestStruct()
        {
            var queue = new BlockingCollection<DispatchStruct>();

            for (var i = 0; i < N; i++)
            {
                var message = Next(false);
                queue.Add(new DispatchStruct((m) => { }, message));
            }

            queue.CompleteAdding();

            foreach (var dispatch in queue.GetConsumingEnumerable())
            {
                dispatch.Action(dispatch.Message);

                if (queue.IsCompleted) break;
            }
        }

        [Benchmark(Baseline = true)]
        public void TestClass()
        {
            var queue = new BlockingCollection<DispatchClass>();

            for (var i = 0; i < N; i++)
            {
                var message = Next(false);

                var dispatch = new DispatchClass((m) => { }, message);

                queue.Add(dispatch);
            }

            queue.CompleteAdding();

            foreach (var dispatch in queue.GetConsumingEnumerable())
            {
                dispatch.Action(dispatch.Message);

                if (queue.IsCompleted) break;
            }
        }


        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestAdvanced4_2>();
        }

    }
}
