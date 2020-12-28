using System;
using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// Allows pooling objects to minimize construction costs. 
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// Total number of pooled objects.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Number of objects gotten and not returned.
        /// </summary>
        public int UsedCount => Count - UnusedCount;
        /// <summary>
        /// Number of pooled objects ready to be used.
        /// </summary>
        public int UnusedCount => pool.Count;

        private readonly Stack<T> pool = new Stack<T>();
        private readonly Action<T> onGet;
        private readonly Action<T> onReturn;

        /// <param name="onGet">Action to invoke before the object is gotten.</param>
        /// <param name="onReturn">Action to invoke after the object is returned.</param>
        public ObjectPool (Action<T> onGet = default, Action<T> onReturn = default)
        {
            this.onGet = onGet;
            this.onReturn = onReturn;
        }

        /// <summary>
        /// Provides an unused object from the pool if there is one,
        /// creates a new instance otherwise.
        /// </summary>
        public T Get ()
        {
            T obj;
            
            if (pool.Count == 0)
            {
                obj = new T();
                Count++;
            }
            else obj = pool.Pop();

            onGet?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Returns the provided object to the pool, so it can be re-used later.
        /// </summary>
        public void Return (T element)
        {
            if (pool.Count > 0 && pool.Contains(element)) return;
            onReturn?.Invoke(element);
            pool.Push(element);
        }
    }
}
