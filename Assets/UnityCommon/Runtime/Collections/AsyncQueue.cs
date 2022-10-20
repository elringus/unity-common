using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityCommon
{
    public class AsyncQueue<T> : IReadOnlyCollection<T>, IDisposable
    {
        public int Count => queue.Count;

        private readonly Queue<T> queue = new Queue<T>();
        private readonly Semaphore semaphore = new Semaphore(0);

        public void Enqueue (T item)
        {
            queue.Enqueue(item);
            semaphore.Release();
        }

        public async UniTask<T> WaitAsync (AsyncToken token)
        {
            while (!token.Canceled)
            {
                await semaphore.WaitAsync(token.CancellationToken);
                return queue.Dequeue();
            }
            throw new OperationCanceledException();
        }

        public void Dispose () => semaphore.Dispose();
        public IEnumerator<T> GetEnumerator () => queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}
