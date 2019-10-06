using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DisruptorPlayground.Inlining
{
    public static class Program
    {
        public interface IDto
        {
            byte[] Data { get; }
        }


        public readonly struct Dto : IDto
        {
            public Dto(byte[] data)
            {
                Data = data;
            }

            public byte[] Data { get;  }
        }

        public class Dto2 : IDto
        {
            public Dto2(byte[] data)
            {
                Data = data;
            }

            public byte[] Data { get; set; }
        }

        public class Processor<T> where T : IDto
        {
            

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ProcessInlined(ref T dto)
            {
                byte[] copy = new byte[dto.Data.Length];

                Array.Copy(dto.Data, copy, dto.Data.Length);
            }

            public void Process(ref T dto)
            {
                byte[] copy = new byte[dto.Data.Length];

                Array.Copy(dto.Data, copy, dto.Data.Length);
            }


        }

        [MemoryDiagnoser]
        public class TestInlining
        {
            private readonly Random _rand;

            public TestInlining()
            {
                _rand = new Random();
            }

            [Benchmark(Baseline = true)]
            public void ClassNotInlined()
            {
                var processor = new Processor<Dto2>();
                byte[] data = new byte[5];

                for (var i = 0; i < 100; i++)
                {
                    _rand.NextBytes(data);

                    var dto = new Dto2(data);
                    processor.Process(ref dto);
                }

            }

            [Benchmark]
            public void ClassInlined()
            {
                var processor = new Processor<Dto2>();
                byte[] data = new byte[5];

                for (var i = 0; i < 100; i++)
                {
                    _rand.NextBytes(data);

                    var dto = new Dto2(data);
                    processor.ProcessInlined(ref dto);
                }
            }


            [Benchmark]
            public void StructNotInlined()
            {
                var processor = new Processor<Dto>();
                byte[] data = new byte[5];

                for (var i = 0; i < 100; i++)
                {
                    _rand.NextBytes(data);

                    var dto = new Dto(data);
                    processor.Process(ref dto);
                }
            }


            [Benchmark]
            public void StructInlined()
            {
                var processor = new Processor<Dto>();
                byte[] data = new byte[5];

                for (var i = 0; i < 100; i++)
                {
                    var dto = new Dto(data);
                    processor.ProcessInlined(ref dto);
                }
            }





        }

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TestInlining>();
        }


    }


}
