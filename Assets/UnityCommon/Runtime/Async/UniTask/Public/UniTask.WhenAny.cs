using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace UnityCommon
{
    public readonly partial struct UniTask
    {
        // UniTask

        public static async UniTask<(bool hasResultLeft, T0 result)> WhenAny<T0> (UniTask<T0> task0, UniTask task1)
        {
            return await new UnitWhenAnyPromise<T0>(task0, task1);
        }

        public static async UniTask<(int winArgumentIndex, T result)> WhenAny<T> (params UniTask<T>[] tasks)
        {
            return await new WhenAnyPromise<T>(tasks);
        }

        /// <summary>Return value is winArgumentIndex</summary>
        public static async UniTask<int> WhenAny (params UniTask[] tasks)
        {
            return await new WhenAnyPromise(tasks);
        }

        private class UnitWhenAnyPromise<T0>
        {
            private T0 result0;
            private ExceptionDispatchInfo exception;
            private Action whenComplete;
            private int completeCount;
            private int winArgumentIndex;

            private bool IsCompleted => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public UnitWhenAnyPromise (UniTask<T0> task0, UniTask task1)
            {
                this.whenComplete = null;
                this.exception = null;
                this.completeCount = 0;
                this.winArgumentIndex = -1;
                this.result0 = default;

                RunTask0(task0).Forget();
                RunTask1(task1).Forget();
            }

            private void TryCallContinuation ()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                action?.Invoke();
            }

            private async UniTaskVoid RunTask0 (UniTask<T0> task)
            {
                T0 value;
                try
                {
                    value = await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    result0 = value;
                    Volatile.Write(ref winArgumentIndex, 0);
                    TryCallContinuation();
                }
            }

            private async UniTaskVoid RunTask1 (UniTask task)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    Volatile.Write(ref winArgumentIndex, 1);
                    TryCallContinuation();
                }
            }

            public Awaiter GetAwaiter ()
            {
                return new Awaiter(this);
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public struct Awaiter : ICriticalNotifyCompletion
            {
                private UnitWhenAnyPromise<T0> parent;

                public Awaiter (UnitWhenAnyPromise<T0> parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsCompleted;

                public (bool, T0) GetResult ()
                {
                    parent.exception?.Throw();

                    return (parent.winArgumentIndex == 0, parent.result0);
                }

                public void OnCompleted (Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted (Action continuation)
                {
                    parent.whenComplete = continuation;
                    if (IsCompleted)
                    {
                        var action = Interlocked.Exchange(ref parent.whenComplete, null);
                        action?.Invoke();
                    }
                }
            }
        }

        private class WhenAnyPromise<T>
        {
            private T result;
            private int completeCount;
            private int winArgumentIndex;
            private Action whenComplete;
            private ExceptionDispatchInfo exception;

            public bool IsComplete => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public WhenAnyPromise (UniTask<T>[] tasks)
            {
                this.completeCount = 0;
                this.winArgumentIndex = -1;
                this.whenComplete = null;
                this.exception = null;
                this.result = default;

                for (int i = 0; i < tasks.Length; i++)
                {
                    RunTask(tasks[i], i).Forget();
                }
            }

            private async UniTaskVoid RunTask (UniTask<T> task, int index)
            {
                T value;
                try
                {
                    value = await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    result = value;
                    Volatile.Write(ref winArgumentIndex, index);
                    TryCallContinuation();
                }
            }

            private void TryCallContinuation ()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                action?.Invoke();
            }

            public Awaiter GetAwaiter ()
            {
                return new Awaiter(this);
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public struct Awaiter : ICriticalNotifyCompletion
            {
                private WhenAnyPromise<T> parent;

                public Awaiter (WhenAnyPromise<T> parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsComplete;

                public (int, T) GetResult ()
                {
                    parent.exception?.Throw();

                    return (parent.winArgumentIndex, parent.result);
                }

                public void OnCompleted (Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted (Action continuation)
                {
                    parent.whenComplete = continuation;
                    if (IsCompleted)
                    {
                        var action = Interlocked.Exchange(ref parent.whenComplete, null);
                        action?.Invoke();
                    }
                }
            }
        }

        private class WhenAnyPromise
        {
            private int completeCount;
            private int winArgumentIndex;
            private Action whenComplete;
            private ExceptionDispatchInfo exception;

            public bool IsComplete => exception != null || Volatile.Read(ref winArgumentIndex) != -1;

            public WhenAnyPromise (UniTask[] tasks)
            {
                this.completeCount = 0;
                this.winArgumentIndex = -1;
                this.whenComplete = null;
                this.exception = null;

                for (int i = 0; i < tasks.Length; i++)
                {
                    RunTask(tasks[i], i).Forget();
                }
            }

            private async UniTaskVoid RunTask (UniTask task, int index)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                    TryCallContinuation();
                    return;
                }

                var count = Interlocked.Increment(ref completeCount);
                if (count == 1)
                {
                    Volatile.Write(ref winArgumentIndex, index);
                    TryCallContinuation();
                }
            }

            private void TryCallContinuation ()
            {
                var action = Interlocked.Exchange(ref whenComplete, null);
                if (action != null)
                {
                    action.Invoke();
                }
            }

            public Awaiter GetAwaiter ()
            {
                return new Awaiter(this);
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public struct Awaiter : ICriticalNotifyCompletion
            {
                private WhenAnyPromise parent;

                public Awaiter (WhenAnyPromise parent)
                {
                    this.parent = parent;
                }

                public bool IsCompleted => parent.IsComplete;

                public int GetResult ()
                {
                    parent.exception?.Throw();

                    return parent.winArgumentIndex;
                }

                public void OnCompleted (Action continuation)
                {
                    UnsafeOnCompleted(continuation);
                }

                public void UnsafeOnCompleted (Action continuation)
                {
                    parent.whenComplete = continuation;
                    if (IsCompleted)
                    {
                        var action = Interlocked.Exchange(ref parent.whenComplete, null);
                        action?.Invoke();
                    }
                }
            }
        }
    }
}
