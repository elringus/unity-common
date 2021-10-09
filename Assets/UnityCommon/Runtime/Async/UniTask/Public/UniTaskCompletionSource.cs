using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityCommon.Async;
using UnityCommon.Async.Internal;

namespace UnityCommon
{
    internal class ExceptionHolder
    {
        private ExceptionDispatchInfo exception;
        private bool calledGet = false;

        public ExceptionHolder (ExceptionDispatchInfo exception)
        {
            this.exception = exception;
        }

        public ExceptionDispatchInfo GetException ()
        {
            if (!calledGet)
            {
                calledGet = true;
                GC.SuppressFinalize(this);
            }
            return exception;
        }

        ~ExceptionHolder ()
        {
            UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
        }
    }

    public interface IResolvePromise
    {
        bool TrySetResult ();
    }

    public interface IResolvePromise<T>
    {
        bool TrySetResult (T value);
    }

    public interface IRejectPromise
    {
        bool TrySetException (Exception exception);
    }

    public interface ICancelPromise
    {
        bool TrySetCanceled ();
    }

    public interface IPromise<T> : IResolvePromise<T>, IRejectPromise, ICancelPromise { }

    public interface IPromise : IResolvePromise, IRejectPromise, ICancelPromise { }

    public class UniTaskCompletionSource : IAwaiter, IPromise
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state = 0;
        private bool handled = false;
        private ExceptionHolder exception;
        private object continuation; // action or list

        AwaiterStatus IAwaiter.Status => (AwaiterStatus)state;

        bool IAwaiter.IsCompleted => state != Pending;

        public UniTask Task => new UniTask(this);

        public UniTaskCompletionSource ()
        {
            TaskTracker.TrackActiveTask(this, 2);
        }

        [Conditional("UNITY_EDITOR")]
        internal void MarkHandled ()
        {
            if (!handled)
            {
                handled = true;
                TaskTracker.RemoveTracking(this);
            }
        }

        void IAwaiter.GetResult ()
        {
            MarkHandled();

            if (state == Succeeded)
            {
                return;
            }
            else if (state == Faulted)
            {
                exception.GetException().Throw();
            }
            else if (state == Canceled)
            {
                exception?.GetException().Throw(); // guranteed operation canceled exception.

                throw new OperationCanceledException();
            }
            else // Pending
            {
                throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted (Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
            else
            {
                var c = continuation;
                if (c is Action)
                {
                    var list = new List<Action>();
                    list.Add((Action)c);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, c) == c)
                    {
                        goto TRYINVOKE;
                    }
                }

                var l = (List<Action>)continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
        }

        private void TryInvokeContinuation ()
        {
            var c = Interlocked.Exchange(ref continuation, null);
            if (c != null)
            {
                if (c is Action)
                {
                    ((Action)c).Invoke();
                }
                else
                {
                    var l = (List<Action>)c;
                    var cnt = l.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        l[i].Invoke();
                    }
                }
            }
        }

        public bool TrySetResult ()
        {
            if (Interlocked.CompareExchange(ref state, Succeeded, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetException (Exception exception)
        {
            if (Interlocked.CompareExchange(ref state, Faulted, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetCanceled ()
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetCanceled (OperationCanceledException exception)
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        void INotifyCompletion.OnCompleted (Action continuation)
        {
            ((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
        }
    }

    public class UniTaskCompletionSource<T> : IAwaiter<T>, IPromise<T>
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state = 0;
        private T value;
        private bool handled = false;
        private ExceptionHolder exception;
        private object continuation; // action or list

        bool IAwaiter.IsCompleted => state != Pending;

        public UniTask<T> Task => new UniTask<T>(this);
        public UniTask UnitTask => new UniTask(this);

        AwaiterStatus IAwaiter.Status => (AwaiterStatus)state;

        public UniTaskCompletionSource ()
        {
            TaskTracker.TrackActiveTask(this, 2);
        }

        [Conditional("UNITY_EDITOR")]
        internal void MarkHandled ()
        {
            if (!handled)
            {
                handled = true;
                TaskTracker.RemoveTracking(this);
            }
        }

        T IAwaiter<T>.GetResult ()
        {
            MarkHandled();

            if (state == Succeeded)
            {
                return value;
            }
            else if (state == Faulted)
            {
                exception.GetException().Throw();
            }
            else if (state == Canceled)
            {
                if (exception != null)
                {
                    exception.GetException().Throw(); // guranteed operation canceled exception.
                }

                throw new OperationCanceledException();
            }
            else // Pending
            {
                throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }

            return default;
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted (Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
            else
            {
                var c = continuation;
                if (c is Action)
                {
                    var list = new List<Action>();
                    list.Add((Action)c);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, c) == c)
                    {
                        goto TRYINVOKE;
                    }
                }

                var l = (List<Action>)continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
        }

        private void TryInvokeContinuation ()
        {
            var c = Interlocked.Exchange(ref continuation, null);
            if (c != null)
            {
                if (c is Action)
                {
                    ((Action)c).Invoke();
                }
                else
                {
                    var l = (List<Action>)c;
                    var cnt = l.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        l[i].Invoke();
                    }
                }
            }
        }

        public bool TrySetResult (T value)
        {
            if (Interlocked.CompareExchange(ref state, Succeeded, Pending) == Pending)
            {
                this.value = value;
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetException (Exception exception)
        {
            if (Interlocked.CompareExchange(ref state, Faulted, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetCanceled ()
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public bool TrySetCanceled (OperationCanceledException exception)
        {
            if (Interlocked.CompareExchange(ref state, Canceled, Pending) == Pending)
            {
                this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(exception));
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        void IAwaiter.GetResult ()
        {
            ((IAwaiter<T>)this).GetResult();
        }

        void INotifyCompletion.OnCompleted (Action continuation)
        {
            ((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
        }
    }
}
