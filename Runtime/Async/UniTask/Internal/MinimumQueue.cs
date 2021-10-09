using System;
using System.Runtime.CompilerServices;

namespace UnityCommon.Async.Internal
{
    // optimized version of Standard Queue<T>.
    internal class MinimumQueue<T>
    {
        private const int MinimumGrow = 4;
        private const int GrowFactor = 200;

        private T[] array;
        private int head;
        private int tail;
        private int size;

        public MinimumQueue (int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            array = new T[capacity];
            head = tail = size = 0;
        }

        public int Count
        {
            #if NET_4_6 || NET_STANDARD_2_0
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            #endif
            get { return size; }
        }

        public T Peek ()
        {
            if (size == 0) ThrowForEmptyQueue();
            return array[head];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue (T item)
        {
            if (size == array.Length)
            {
                Grow();
            }

            array[tail] = item;
            MoveNext(ref tail);
            size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue ()
        {
            if (size == 0) ThrowForEmptyQueue();

            int head = this.head;
            T[] array = this.array;
            T removed = array[head];
            array[head] = default;
            MoveNext(ref this.head);
            size--;
            return removed;
        }

        private void Grow ()
        {
            int newCapacity = (int)(array.Length * (long)GrowFactor / 100);
            if (newCapacity < array.Length + MinimumGrow)
            {
                newCapacity = array.Length + MinimumGrow;
            }
            SetCapacity(newCapacity);
        }

        private void SetCapacity (int capacity)
        {
            T[] newArray = new T[capacity];
            if (size > 0)
            {
                if (head < tail)
                {
                    Array.Copy(array, head, newArray, 0, size);
                }
                else
                {
                    Array.Copy(array, head, newArray, 0, array.Length - head);
                    Array.Copy(array, 0, newArray, array.Length - head, tail);
                }
            }

            array = newArray;
            head = 0;
            tail = size == capacity ? 0 : size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext (ref int index)
        {
            int tmp = index + 1;
            if (tmp == array.Length)
            {
                tmp = 0;
            }
            index = tmp;
        }

        private void ThrowForEmptyQueue ()
        {
            throw new InvalidOperationException("EmptyQueue");
        }
    }
}
