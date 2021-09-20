using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;
using UniRx.Async;

namespace UnityCommon
{
    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
    public class AsyncQueue<T> : IDisposable, IReadOnlyCollection<T>
    {
        public int Count => queue.Count;

        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

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
                if (queue.TryDequeue(out var message)) return message;
            }
            throw new OperationCanceledException();
        }

        public void Dispose () => semaphore.Dispose();
        public IEnumerator<T> GetEnumerator () => queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}
