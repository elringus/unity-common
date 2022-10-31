using System;
using System.Collections.Concurrent;
using System.Threading;

namespace UnityCommon
{
    /// <summary>
    /// Replacement for <see cref="SemaphoreSlim"/> that runs on Unity scheduler.
    /// Required for platforms without threading support, such as WebGL.
    /// </summary>
    public class Semaphore : IDisposable
    {
        private readonly ConcurrentQueue<UniTaskCompletionSource> waiters = new ConcurrentQueue<UniTaskCompletionSource>();
        private readonly int maxCount;
        private int count;

        public Semaphore (int initialCount, int maxCount = int.MaxValue)
        {
            count = initialCount;
            this.maxCount = maxCount;
        }

        public UniTask WaitAsync () => WaitAsync(CancellationToken.None);

        public UniTask WaitAsync (CancellationToken token)
        {
            if (count > 0)
            {
                count--;
                return UniTask.CompletedTask;
            }

            var tcs = new UniTaskCompletionSource();
            if (token.CanBeCanceled)
                token.Register(() => tcs.TrySetCanceled());
            waiters.Enqueue(tcs);
            return tcs.Task;
        }

        public void Release () => Release(1);

        public void Release (int releaseCount)
        {
            for (int i = 0; i < releaseCount; i++)
            {
                if (count + 1 > maxCount) break;
                if (waiters.Count > 0) ReleaseWaiter();
                count++;
            }
        }

        public void Dispose ()
        {
            while (!waiters.IsEmpty)
            {
                waiters.TryDequeue(out var waiter);
                waiter.TrySetCanceled();
            }
        }

        private void ReleaseWaiter ()
        {
            var waiter = default(UniTaskCompletionSource);
            while (waiter is null)
                waiters.TryDequeue(out waiter);
            waiter.TrySetResult();
        }
    }
}
