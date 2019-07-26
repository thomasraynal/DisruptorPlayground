using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.POC
{
    public class MyRingBuffer
    {
        public int Size;
        public readonly byte[][] Buffer;

        public MyRingBuffer(int size)
        {
            Size = size;
            Buffer = new byte[size][];
        }
    }
}
