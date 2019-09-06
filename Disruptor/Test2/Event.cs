using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.Advanced2
{
    public class Event
    {
        public Event(int size)
        {
            _size = size;
            Data = new byte[_size];
        }

        private readonly int _size;
        public byte[] Data;
        public int Counter;

        public void Reset()
        {
            for (var i = 0; i < _size; i++)
            {
                Data[i] = 0;
            }
        }
    }
}
