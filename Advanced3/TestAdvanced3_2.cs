//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Running;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace DisruptorPlayground.Advanced3
//{
//    public static class TestAdvanced3_2
//    {
//        struct C : IEquatable<C>
//        {
//            public C(int data)
//            {
//                Data = data;
//            }

//            int Data { get; }

//            public bool Equals(C other)
//            {
//                return this.Data.Equals(other);
//            }

//            public override int GetHashCode()
//            {
//                return Data.GetHashCode();
//            }
//        }

//        struct C2 : IEquatable<C2>
//        {
//             static readonly IEqualityComparer<int> Eq = EqualityComparer<int>.Default;

//            public C2(int data)
//            {
//                Data = data;
//            }

//            public int Data { get; }

//            public bool Equals(C2 other)
//            {
//                return Eq.Equals(this.Data, other.Data);
//            }

//            public override int GetHashCode()
//            {
//                return Eq.GetHashCode(Data);
//            }
//        }

//        [CoreJob(baseline: true)]
//        public class BoxingBenchmark3
//        {
//            [Params(1, 100)]
//            public int N;
//            private C[] _classes;
//            private S[] _structs;

//            public class C : IDisposable
//            {
//                public void Dispose()
//                {
//                }
//            }

//            public struct S : IDisposable
//            {
//                public void Dispose()
//                {
//                }
//            }

//            [GlobalSetup]
//            public void Setup()
//            {
//                _classes = Enumerable.Range(0, N).Select(i => new C()).ToArray();
//                _structs = Enumerable.Range(0, N).Select(i => new S()).ToArray();
//            }

//            public void Dispose(C disposable)
//            {
//                disposable.Dispose();
//            }

//            public void DoDispose(S disposable)
//            {
//                disposable.Dispose();
//            }

//            public void DisposeAsDisposable(IDisposable disposable)
//            {
//                disposable.Dispose();
//            }

//            [Benchmark(Baseline = true)]
//            public void Boxing()
//            {
//                for (var i = 0; i < N; i++)
//                {
//                    DisposeAsDisposable(_classes[i]);
//                }

//            }

//            [Benchmark]
//            public void NoBoxing()
//            {
//                for (var i = 0; i < N; i++)
//                {
//                   Dispose(_classes[i]);
//                }

//            }

//            [Benchmark]
//            public void BoxingStruct()
//            {
//                for (var i = 0; i < N; i++)
//                {
//                    DisposeAsDisposable(_structs[i]);
//                }

//            }

//            [Benchmark]
//            public void NoBoxingStruct()
//            {
//                for (var i = 0; i < N; i++)
//                {
//                    DoDispose(_structs[i]);
//                }
//            }
//        }
      
//        [CoreJob(baseline: true)]
//        public class BoxingBenchmark2
//        {
//            private List<int> _list;

//            public BoxingBenchmark2()
//            {
//                _list = Enumerable.Range(0, 1000).ToList();
//            }

//            [Benchmark(Baseline = true)]
//            public void Boxing()
//            {
//                DoBoxing(_list);
//            }

//            [Benchmark]
//            public void NoBoxing()
//            {
//                DoNoBoxing(_list);
//            }


//            void DoNoBoxing(List<int> list)
//            {
//                foreach (var i in list)
//                {
//                }
//            }

//            void DoBoxing(IList<int> list)
//            {
//                foreach (var i in list)
//                {
//                }
//            }
//        }

//        [CoreJob(baseline: true)]
//        public class BoxingBenchmark
//        {
//            [Params(1, 100)]
//            public int N;

//            private C[] _boxing;
//            private C2[] _noBoxing;

//            [GlobalSetup]
//            public void Setup()
//            {
//                _boxing = Enumerable.Range(0, N).Select(i => new C(i)).ToArray();
//                _noBoxing = Enumerable.Range(0, N).Select(i => new C2(i)).ToArray();
//            }


//            [Benchmark(Baseline = true)]
//            public void Boxing()
//            {
//                var c = new C(200);

//                for (var i = 0; i < N; i++)
//                {
//                    var equals = c.Equals(_boxing[i]);
//                }
//            }

//            [Benchmark]
//            public void NoBoxing()
//            {
//                var c = new C2(200);

//                for (var i = 0; i < N; i++)
//                {
//                    var equals = c.Equals(_noBoxing[i]);
//                }
//            }
//        }

//        public static void Main(string[] args)
//        {
//            BenchmarkRunner.Run<BoxingBenchmark3>();
//        }

//    }
//}
