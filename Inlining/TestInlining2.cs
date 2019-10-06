using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace DisruptorPlayground.Inlining2
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Storage16
    {
    }

    //https://medium.com/@ocoanet/benchmarking-adventures-part-1-avoiding-boxing-fc7756385ffb
    public class Storage<T>
    {
        private Storage16 _valueStorage;
        private object _defaultStorage;

        public Storage(T obj)
        {
            if(!RuntimeHelpers.IsReferenceOrContainsReferences<T>() && Unsafe.SizeOf<T>() <= 16)
            {
                Unsafe.As<Storage16, T>(ref _valueStorage) = obj;
            }
            else
            {
                _defaultStorage = obj;
            }
        }
    }

    public static class Program2
    {
        public class TestInlining2
        {

        }
        public static void Main(string[] args)
        {
            var i = 12;

            var storage = new Storage<int>(i);
        }

    }
}
