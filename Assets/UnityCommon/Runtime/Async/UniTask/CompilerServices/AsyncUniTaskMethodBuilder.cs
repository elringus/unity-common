using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace UnityCommon.Async.CompilerServices
{
    public struct AsyncUniTaskMethodBuilder
    {
        private UniTaskCompletionSource promise;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncUniTaskMethodBuilder Create ()
        {
            var builder = new AsyncUniTaskMethodBuilder();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public UniTask Task
        {
            get
            {
                if (promise != null)
                {
                    return promise.Task;
                }

                if (moveNext == null)
                {
                    return UniTask.CompletedTask;
                }
                promise = new UniTaskCompletionSource();
                return promise.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException (Exception exception)
        {
            if (promise == null)
            {
                promise = new UniTaskCompletionSource();
            }
            if (exception is OperationCanceledException ex)
            {
                promise.TrySetCanceled(ex);
            }
            else
            {
                promise.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult ()
        {
            if (moveNext == null) { }
            else
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource();
                }
                promise.TrySetResult();
            }
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.OnCompleted(moveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine> (ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine (IAsyncStateMachine stateMachine) { }

        #if UNITY_EDITOR
        // Important for IDE debugger.
        private object debuggingId;
        private object ObjectIdForDebugger
        {
            get
            {
                if (debuggingId == null)
                {
                    debuggingId = new object();
                }
                return debuggingId;
            }
        }
        #endif
    }

    public struct AsyncUniTaskMethodBuilder<T>
    {
        private T result;
        private UniTaskCompletionSource<T> promise;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncUniTaskMethodBuilder<T> Create ()
        {
            var builder = new AsyncUniTaskMethodBuilder<T>();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public UniTask<T> Task
        {
            get
            {
                if (promise != null)
                {
                    return new UniTask<T>(promise);
                }

                if (moveNext == null)
                {
                    return new UniTask<T>(result);
                }
                promise = new UniTaskCompletionSource<T>();
                return new UniTask<T>(promise);
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException (Exception exception)
        {
            if (promise == null)
            {
                promise = new UniTaskCompletionSource<T>();
            }
            if (exception is OperationCanceledException ex)
            {
                promise.TrySetCanceled(ex);
            }
            else
            {
                promise.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult (T result)
        {
            if (moveNext == null)
            {
                this.result = result;
            }
            else
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource<T>();
                }
                promise.TrySetResult(result);
            }
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource<T>(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.OnCompleted(moveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (promise == null)
                {
                    promise = new UniTaskCompletionSource<T>(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine> (ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine (IAsyncStateMachine stateMachine) { }

        #if UNITY_EDITOR
        // Important for IDE debugger.
        private object debuggingId;
        private object ObjectIdForDebugger
        {
            get
            {
                if (debuggingId == null)
                {
                    debuggingId = new object();
                }
                return debuggingId;
            }
        }
        #endif
    }
}
