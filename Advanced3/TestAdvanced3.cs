using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Disruptor;
using Disruptor.Dsl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorPlayground.Advanced3
{
    //https://medium.com/@ocoanet/improving-net-disruptor-performance-part-2-5bf456cd595f
   // [TestFixture]
    public class Program
    {
        public interface IHasValues
        {
            int Value1 { get; }
            int Value2 { get; }
        }

        public static int SumValues<T>(T source) where T : IHasValues
        {
            return source.Value1 + source.Value2;
        }

        public class C : IHasValues
        {
            public C(int value1, int value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public int Value1 { get;  }


            public int Value2 { get; }

        }

        public struct S : IHasValues
        {
            public S(int value1, int value2)
            {
                Value1 = value1;
                Value2 = value2;
            }
            public int Value1 { get;  }
            public int Value2 { get;  }
        }

        [CoreJob(baseline: true)]
        public class StructVsClassBenchmark
        {
            [Params(1,100)]
            public int N;

            private IHasValues[] _proxies;
            private IHasValues[] _structs;
            private IHasValues[] _classes;

            [GlobalSetup]
            public void Setup()
            {
                _proxies = Enumerable.Range(0, N).Select(i => StructProxy.CreateProxyInstance<IHasValues>(new C(i, i + 1))).ToArray();
                _structs = Enumerable.Range(0, N).Select(i => new S(i, i + 1)).Cast<IHasValues>().ToArray();
                _classes = Enumerable.Range(0, N).Select(i => new C(i, i + 1)).ToArray();
            }

            public void DoWork(IHasValues[] values)
            {
                for (var i = 0; i < N; i++)
                {
                    SumValues(values[i]);
                }
            }


            [Benchmark(Baseline = true)]
            public void Class()
            {
                DoWork(_classes);
            }

            [Benchmark]
            public void Proxy()
            {
                DoWork(_proxies);
            }

            [Benchmark]
            public void Struct()
            {
                DoWork(_structs);
            }
        }



        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<StructVsClassBenchmark>();
        }

    }


}
