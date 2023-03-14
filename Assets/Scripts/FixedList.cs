using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace RTLOL
{
    public struct FixedIterator<T> : IEnumerator<FixedIterator<T>>
    {
        public ref T Value => ref m_list[m_index];
        public int Index => m_index;

        public FixedIterator<T> Current => this;
        object IEnumerator.Current => this;

        private FixedList<T> m_list;
        private int m_index;

        public FixedIterator(FixedList<T> list)
        {
            m_list = list;
            m_index = -1;
        }

        public bool MoveNext()
        {
            return ++m_index < m_list.Count;
        }

        public void Reset()
        {
            m_index = -1;
        }

        public void Dispose()
        {
        }
    }

    public class FixedList<T> : IEnumerable<FixedIterator<T>>
    {
        public delegate bool FixedPredicate<TContext>(TContext context, ref T element);

        public int Count => m_count;
        public int Capacity => m_capacity;

        private int m_capacity;
        private int m_count;
        private T[] m_array;

        public FixedList(int capacity)
        {
            m_capacity = capacity;
            m_count = 0;
            m_array = new T[capacity];
        }

        public ref T this[int index]
        {
            get
            {
                Assert.IsTrue(index < m_count, $"{nameof(index)} {index} is out of range");
                return ref m_array[index];
            }
        }

        public void Add(in T value)
        {
            Assert.IsTrue(m_count < m_capacity, $"Capacity {m_capacity} has been reached");
            m_array[m_count] = value;
            ++m_count;
        }

        public void RemoveAt(int index)
        {
            Assert.IsTrue(index < m_count, $"{nameof(index)} {index} is out of range");
            --m_count;
            for (int i = index; i < m_count; ++i)
                m_array[i] = m_array[i + 1];
        }

        public void Clear()
        {
            m_count = 0;
        }

        public IEnumerator<FixedIterator<T>> GetEnumerator()
        {
            return new FixedIterator<T>(this);
        }

        public Span<T> AsSpan()
        {
            return new Span<T>(m_array, 0, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FixedIterator<T>(this);
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            int dst = 0;

            for (int src = 0; src < m_count; ++src)
            {
                if (!predicate(m_array[src]))
                {
                    m_array[dst] = m_array[src];
                    ++dst;
                }
            }

            m_count = dst;
        }

        public void RemoveAll<TContext>(TContext context, FixedPredicate<TContext> predicate)
        {
            int dst = 0;

            for (int src = 0; src < m_count; ++src)
            {
                if (!predicate(context, ref m_array[src]))
                {
                    m_array[dst] = m_array[src];
                    ++dst;
                }
            }

            m_count = dst;
        }
    }
}