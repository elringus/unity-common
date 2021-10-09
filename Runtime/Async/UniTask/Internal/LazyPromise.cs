using System;
using System.Threading;

namespace UnityCommon.Async.Internal
{
    internal sealed class LazyPromise : IAwaiter
    {
        private Func<UniTask> factory;
        private UniTask value;

        public LazyPromise (Func<UniTask> factory)
        {
            this.factory = factory;
        }

        private void Create ()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null)
            {
                value = f();
            }
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        public void GetResult ()
        {
            Create();
            value.GetResult();
        }

        void IAwaiter.GetResult ()
        {
            GetResult();
        }

        public void UnsafeOnCompleted (Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted (Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }
    }

    internal sealed class LazyPromise<T> : IAwaiter<T>
    {
        private Func<UniTask<T>> factory;
        private UniTask<T> value;

        public LazyPromise (Func<UniTask<T>> factory)
        {
            this.factory = factory;
        }

        private void Create ()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null)
            {
                value = f();
            }
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        public T GetResult ()
        {
            Create();
            return value.Result;
        }

        void IAwaiter.GetResult ()
        {
            GetResult();
        }

        public void UnsafeOnCompleted (Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted (Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }
    }
}
