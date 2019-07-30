using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisruptorPlayground.Advanced3
{
    public static class TestAdvanced3_3
    {
        [CoreJob(baseline: true)]
        public class StrucVsClass
        {
            [Params(1, 100)]
            public int N;

            private SmallClass[] _smallClasses;
            private LargeClass[] _largeClasses;
            private SmallStruct[] _smallStructs;
            private LargeStruct[] _largeStructs;

            class SmallClass
            {
                public int Val;

                public SmallClass(int val)
                {
                    Val = val;
                }
            }

            class LargeClass
            {
                public int Val;
                public int Val2;
                public int Val3;
                public int Val4;
                public int Val5;
                public int Val6;

                public LargeClass(int val, int val2, int val3, int val4, int val5, int val6)
                {
                    Val = val;
                    Val2 = val2;
                    Val3 = val3;
                    Val4 = val4;
                    Val5 = val5;
                    Val6 = val6;
                }
            }

            struct SmallStruct
            {
                public int Val;

                public SmallStruct(int val)
                {
                    Val = val;
                }
            }

            struct LargeStruct
            {
                public int Val;
                public int Val2;
                public int Val3;
                public int Val4;
                public int Val5;
                public int Val6;

                public LargeStruct(int val, int val2, int val3, int val4, int val5, int val6)
                {
                    Val = val;
                    Val2 = val2;
                    Val3 = val3;
                    Val4 = val4;
                    Val5 = val5;
                    Val6 = val6;
                }
            }

            [GlobalSetup]
            public void Setup()
            {
                _smallClasses = Enumerable.Range(0, N).Select(i => new SmallClass(i)).ToArray();
                _largeClasses = Enumerable.Range(0, N).Select(i => new LargeClass(i, i + 1, i + 2, i + 3, i + 4, i + 5)).ToArray();
                _smallStructs = Enumerable.Range(0, N).Select(i => new SmallStruct(i)).ToArray();
                _largeStructs = Enumerable.Range(0, N).Select(i => new LargeStruct(i, i + 1, i + 2, i + 3, i + 4, i + 5)).ToArray();
            }

            int Get(SmallClass @class)
            {
                return @class.Val;
            }

            int Get(LargeClass @class)
            {
                return @class.Val;
            }

            int Get(ref SmallStruct @class)
            {
                return @class.Val;
            }

            int Get(ref LargeStruct @class)
            {
                return @class.Val;
            }


            [Benchmark(Baseline = true)]
            public void SmallClasses()
            {
                for (var i = 0; i < N; i++)
                {
                    Get(_smallClasses[i]);
                }

            }


            [Benchmark]
            public void SmallStructs()
            {
                for (var i = 0; i < N; i++)
                {
                    Get(ref _smallStructs[i]);
                }

            }

            [Benchmark]
            public void LargeClasses()
            {
                for (var i = 0; i < N; i++)
                {
                    Get(_largeClasses[i]);
                }

            }

            [Benchmark]
            public void LargeStructs()
            {
                for (var i = 0; i < N; i++)
                {
                    Get(ref _largeStructs[i]);
                }

            }

        }

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<StrucVsClass>();
        }


    }
}
