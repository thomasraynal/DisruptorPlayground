using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DisruptorPlayground.FalseSharing
{
    [StructLayout(LayoutKind.Auto)]
    public struct AlignedDoubleAuto
    {
        public byte B;
        public double D;
        public int I;
    }

    public struct AlignedDoubleManaged
    {
        public byte B;
        public string C;
        public double D;
        public int I;
    }


    public struct AlignedDouble
    {
        //1
        public byte B;
        //7 (padding)
        //8
        public double D;
        //4
        public int I;
        //4 (padding)

    }

    public struct AlignedInt
    {
        public int I;
        public byte B;
        //3 (padding)
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DiscriminatedUnion
    {
        [FieldOffset(0)]
        public bool Bool;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public int Integer;
    }

    [TestFixture]
    [MemoryDiagnoser]
    public class TestFalseSharing1
    {
        [Test]
        public void TestStruct()
        {

            //var cacheline = Windows.GetSize();

            var s = new DiscriminatedUnion();
            s.Byte = 1;
            s.Integer = 5;

            var d = Unsafe.SizeOf<AlignedInt>();

            var a = Unsafe.SizeOf<AlignedDouble>();
            var b = Unsafe.SizeOf<AlignedDoubleAuto>();
            var c = Unsafe.SizeOf<AlignedDoubleManaged>();
        }

        [Params(1, 16)]
        public int offset;

        [Params(0, 16)]
        public int gap = 0;


        [IterationSetup]
        public void SetUp()
        {
            sharedData = new int[4 * offset + gap * offset];
        }

        public int[] sharedData;
        public int threadsCount = 4;
        public int size = 100_000_000;

        [Benchmark]
        public long DoFalseSharingTest()
        {
            var workers = new Thread[threadsCount];
            for (int i = 0; i < threadsCount; ++i)
            {
                workers[i] = new Thread(new ParameterizedThreadStart(idx =>
                {
                    int index = (int)idx + gap;
                    for (int j = 0; j < size; ++j)
                    {
                        sharedData[index * offset] = sharedData[index * offset] +
                       1;
                    }
                }));
            }
            for (int i = 0; i < threadsCount; ++i)
                workers[i].Start(i);
            for (int i = 0; i < threadsCount; ++i)
                workers[i].Join();
            return 0;
        }

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestFalseSharing1>();
        }
    }

    public class TestFalseSharing2
    {


        struct F
        {
            public int x;
            public int y;
        }

        struct F2
        {

            public long w;
            public int x;
            public object y;
            public object z;
        }


        [StructLayout(LayoutKind.Explicit, Size = 128)]
        struct S
        {
            //4
            [FieldOffset(0)]
            public int x;

            //60 (padding)
            [FieldOffset(64)]

            //4
            public int y;

            //60 (padding)
        }



        [StructLayout(LayoutKind.Explicit, Size = 192)]
        struct S2
        {
            //8
            [FieldOffset(0)]
            public long w;
            //4
            [FieldOffset(8)]
            public int x;
     
            //padding 52

            [FieldOffset(64)]
            public object y;

            //padding 56

            [FieldOffset(128)]
            public object z;

            //padding 56
        }








        private F f;
        private S s;

        private F2 f2;
        private S2 s2;

        [IterationSetup]
        public void SetUp()
        {
            f = new F();
            s = new S();
            f2 = new F2();
            s2 = new S2();

        }

        [Benchmark]
        public void FalseSharing1()
        {
            var t1 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    ++f.x;
                }

            });

            var t2 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    ++f.y;
                }

            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }

        [Benchmark]
        public void FalseSharing2()
        {
            var t1 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    f2.y = new object();
                }

            });

            var t2 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    f2.z = new object();
                }

            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }




        [Benchmark]
        public void Sharing1()
        {
            var t1 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    ++s.x;
                }

            });

            var t2 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    ++s.y;
                }

            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

        }

        [Benchmark]
        public void Sharing2()
        {
            var t1 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    s2.y = new object();
                }

            });

            var t2 = new Thread(() =>
            {
                for (int i = 0; i < 100_000_000; ++i)
                {
                    s2.z = new object();
                }

            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

        }



        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestFalseSharing2>();
        }
    }

}
