using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityCommon
{
    /// <summary>
    /// Replacement for <see cref="SemaphoreSlim"/> that runs on Unity scheduler.
    /// Required for platforms without threading support, such as WebGL.
    /// </summary>
    public class Semaphore : IDisposable
    {
        private readonly Queue<UniTaskCompletionSource> waiters = new Queue<UniTaskCompletionSource>();
        private int count;

        public Semaphore (int initialCount)
        {
            count = initialCount;
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
                if (waiters.Count > 0)
                    waiters.Dequeue().TrySetResult();
                count++;
            }
        }

        public void Dispose ()
        {
            foreach (var waiter in waiters)
                waiter.TrySetCanceled();
            waiters.Clear();
        }
    }
}
