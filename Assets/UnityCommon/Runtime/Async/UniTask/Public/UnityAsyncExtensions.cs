using System;
using System.Threading;
using UnityCommon.Async;
using UnityCommon.Async.Internal;
using UnityEngine;

namespace UnityCommon
{
    public static class UnityAsyncExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter (this AsyncOperation asyncOperation)
        {
            UniTaskError.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new AsyncOperationAwaiter(asyncOperation);
        }

        public static UniTask ToUniTask (this AsyncOperation asyncOperation)
        {
            UniTaskError.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UniTask(new AsyncOperationAwaiter(asyncOperation));
        }

        public static UniTask ConfigureAwait (this AsyncOperation asyncOperation,
            IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            UniTaskError.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            var awaiter = new AsyncOperationConfiguredAwaiter(asyncOperation, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask(awaiter);
        }

        public static ResourceRequestAwaiter GetAwaiter (this ResourceRequest resourceRequest)
        {
            UniTaskError.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new ResourceRequestAwaiter(resourceRequest);
        }

        public static UniTask<UnityEngine.Object> ToUniTask (this ResourceRequest resourceRequest)
        {
            UniTaskError.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<UnityEngine.Object>(new ResourceRequestAwaiter(resourceRequest));
        }

        public static UniTask<UnityEngine.Object> ConfigureAwait (this ResourceRequest resourceRequest,
            IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellation = default)
        {
            UniTaskError.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            var awaiter = new ResourceRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted) PlayerLoopHelper.AddAction(timing, awaiter);
            return new UniTask<UnityEngine.Object>(awaiter);
        }

        public struct AsyncOperationAwaiter : IAwaiter
        {
            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            private AsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;

            public AsyncOperationAwaiter (AsyncOperation asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                continuationAction = null;
            }

            public void GetResult ()
            {
                if (Status == AwaiterStatus.Succeeded) return;

                if (Status == AwaiterStatus.Pending)
                {
                    if (asyncOperation.isDone) Status = AwaiterStatus.Succeeded;
                    else UniTaskError.ThrowNotYetCompleted();
                }

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null;
                    continuationAction = null;
                }
                else asyncOperation = null;
            }

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                UniTaskError.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AsyncOperationConfiguredAwaiter : IAwaiter, IPlayerLoopItem
        {
            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            private AsyncOperation asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private Action continuation;

            public AsyncOperationConfiguredAwaiter (AsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
            }

            public void GetResult ()
            {
                if (Status == AwaiterStatus.Succeeded) return;
                if (Status == AwaiterStatus.Canceled)
                    UniTaskError.ThrowOperationCanceledException();
                UniTaskError.ThrowNotYetCompleted();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                progress?.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;
                cont?.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                UniTaskError.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        public struct ResourceRequestAwaiter : IAwaiter<UnityEngine.Object>
        {
            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            private ResourceRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private UnityEngine.Object result;

            public ResourceRequestAwaiter (ResourceRequest asyncOperation)
            {
                Status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = Status.IsCompleted() ? null : asyncOperation;
                result = Status.IsCompletedSuccessfully() ? asyncOperation.asset : null;
                continuationAction = null;
            }

            public UnityEngine.Object GetResult ()
            {
                if (Status == AwaiterStatus.Succeeded) return result;

                if (Status == AwaiterStatus.Pending)
                {
                    if (asyncOperation.isDone) Status = AwaiterStatus.Succeeded;
                    else UniTaskError.ThrowNotYetCompleted();
                }

                result = asyncOperation.asset;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null;
                    continuationAction = null;
                }
                else asyncOperation = null;

                return result;
            }

            void IAwaiter.GetResult () => GetResult();

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                UniTaskError.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class ResourceRequestConfiguredAwaiter : IAwaiter<UnityEngine.Object>, IPlayerLoopItem
        {
            public bool IsCompleted => Status.IsCompleted();
            public AwaiterStatus Status { get; private set; }

            private ResourceRequest asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private Action continuation;
            private UnityEngine.Object result;

            public ResourceRequestConfiguredAwaiter (ResourceRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (Status.IsCompletedSuccessfully()) result = asyncOperation.asset;
                if (Status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                continuation = null;
                result = null;
            }

            void IAwaiter.GetResult () => GetResult();

            public UnityEngine.Object GetResult ()
            {
                if (Status == AwaiterStatus.Succeeded) return result;
                if (Status == AwaiterStatus.Canceled)
                    UniTaskError.ThrowOperationCanceledException();
                return UniTaskError.ThrowNotYetCompleted<UnityEngine.Object>();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                progress?.Report(asyncOperation.progress);

                if (asyncOperation.isDone)
                {
                    result = asyncOperation.asset;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                Status = status;
                var cont = continuation;
                continuation = null;
                cancellationToken = CancellationToken.None;
                progress = null;
                asyncOperation = null;
                cont?.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                UniTaskError.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                UniTaskError.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }
    }
}
