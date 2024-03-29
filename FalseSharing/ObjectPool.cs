﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DisruptorPlayground.Advanced5
{
    //Konrad Kokosa - Pro .NET Memory Management
    public class ObjectPool<T> where T : class
    {
        private T firstItem;
        private readonly T[] items;
        private readonly Func<T> generator;
        public ObjectPool(Func<T> generator, int size)
        {
            this.generator = generator ?? throw new ArgumentNullException("generator");
            this.items = new T[size - 1];
        }
        public T Rent()
        {
            // PERF: Examine the first element. If that fails, RentSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional.
            // We will interlock only when we have a candidate. in a worst case  we may miss some recently returned objects.Not a big deal.
            T inst = firstItem;

            if (inst == null || inst != Interlocked.CompareExchange(ref firstItem, null, inst))
            {
                inst = RentSlow();
            }

            return inst;
        }

        public void Return(T item)
        {
            if (firstItem == null)
            {
                // Intentionally not using interlocked here.
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                firstItem = item;
            }
            else
            {
                ReturnSlow(item);
            }
        }
        private T RentSlow()
        {
            for (int i = 0; i < items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized.That is intentional.
                // We will interlock only when we have a candidate. in a worst case we may miss some recently returned objects.Not a big deal.
                T inst = items[i];
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i], null, inst))

                    {
                        return inst;
                    }
                }
            }
            return generator();
        }
        private void ReturnSlow(T obj)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    // Intentionally not using interlocked here.
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i] = obj;
                    break;
                }
            }
        }
    }
}
