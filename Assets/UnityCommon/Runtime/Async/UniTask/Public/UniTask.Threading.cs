using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon.Async;
using UnityCommon.Async.Internal;

namespace UnityCommon
{
    public readonly partial struct UniTask
    {
        /// <summary>
        /// If running on mainthread, do nothing. Otherwise, same as UniTask.Yield(PlayerLoopTiming.Update).
        /// </summary>
        public static SwitchToMainThreadAwaitable SwitchToMainThread ()
        {
            return new SwitchToMainThreadAwaitable();
        }

        public static SwitchToThreadPoolAwaitable SwitchToThreadPool ()
        {
            return new SwitchToThreadPoolAwaitable();
        }

        public static SwitchToTaskPoolAwaitable SwitchToTaskPool ()
        {
            return new SwitchToTaskPoolAwaitable();
        }

        public static SwitchToSynchronizationContextAwaitable SwitchToSynchronizationContext (SynchronizationContext syncContext)
        {
            Error.ThrowArgumentNullException(syncContext, nameof(syncContext));
            return new SwitchToSynchronizationContextAwaitable(syncContext);
        }
    }

    public struct SwitchToMainThreadAwaitable
    {
        public Awaiter GetAwaiter () => new Awaiter();

        public struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted
            {
                get
                {
                    var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (PlayerLoopHelper.MainThreadId == currentThreadId)
                    {
                        return true; // run immediate.
                    }
                    else
                    {
                        return false; // register continuation.
                    }
                }
            }

            public void GetResult () { }

            public void OnCompleted (Action continuation)
            {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                PlayerLoopHelper.AddContinuation(PlayerLoopTiming.Update, continuation);
            }
        }
    }

    public struct SwitchToThreadPoolAwaitable
    {
        public Awaiter GetAwaiter () => new Awaiter();

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly WaitCallback switchToCallback = Callback;

            public bool IsCompleted => false;
            public void GetResult () { }

            public void OnCompleted (Action continuation)
            {
                ThreadPool.UnsafeQueueUserWorkItem(switchToCallback, continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                ThreadPool.UnsafeQueueUserWorkItem(switchToCallback, continuation);
            }

            private static void Callback (object state)
            {
                var continuation = (Action)state;
                continuation();
            }
        }
    }

    public struct SwitchToTaskPoolAwaitable
    {
        public Awaiter GetAwaiter () => new Awaiter();

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly Action<object> switchToCallback = Callback;

            public bool IsCompleted => false;
            public void GetResult () { }

            public void OnCompleted (Action continuation)
            {
                Task.Factory.StartNew(switchToCallback, continuation, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Task.Factory.StartNew(switchToCallback, continuation, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            private static void Callback (object state)
            {
                var continuation = (Action)state;
                continuation();
            }
        }
    }

    public readonly struct SwitchToSynchronizationContextAwaitable
    {
        private readonly SynchronizationContext synchronizationContext;

        public SwitchToSynchronizationContextAwaitable (SynchronizationContext synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        public Awaiter GetAwaiter () => new Awaiter(synchronizationContext);

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly SendOrPostCallback switchToCallback = Callback;
            private readonly SynchronizationContext synchronizationContext;

            public Awaiter (SynchronizationContext synchronizationContext)
            {
                this.synchronizationContext = synchronizationContext;
            }

            public bool IsCompleted => false;
            public void GetResult () { }

            public void OnCompleted (Action continuation)
            {
                synchronizationContext.Post(switchToCallback, continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                synchronizationContext.Post(switchToCallback, continuation);
            }

            private static void Callback (object state)
            {
                var continuation = (Action)state;
                continuation();
            }
        }
    }
}
