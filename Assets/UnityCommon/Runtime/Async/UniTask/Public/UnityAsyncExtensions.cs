using System;
using System.Threading;
using UnityCommon.Async;
using UnityCommon.Async.Internal;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityCommon
{
    public static class UnityAsyncExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter (this AsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new AsyncOperationAwaiter(asyncOperation);
        }

        public static UniTask ToUniTask (this AsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UniTask(new AsyncOperationAwaiter(asyncOperation));
        }

        public static UniTask ConfigureAwait (this AsyncOperation asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));

            var awaiter = new AsyncOperationConfiguredAwaiter(asyncOperation, progress, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(timing, awaiter);
            }
            return new UniTask(awaiter);
        }

        public static ResourceRequestAwaiter GetAwaiter (this ResourceRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new ResourceRequestAwaiter(resourceRequest);
        }

        public static UniTask<UnityEngine.Object> ToUniTask (this ResourceRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<UnityEngine.Object>(new ResourceRequestAwaiter(resourceRequest));
        }

        public static UniTask<UnityEngine.Object> ConfigureAwait (this ResourceRequest resourceRequest, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new ResourceRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(timing, awaiter);
            }
            return new UniTask<UnityEngine.Object>(awaiter);
        }

        public static AssetBundleRequestAwaiter GetAwaiter (this AssetBundleRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new AssetBundleRequestAwaiter(resourceRequest);
        }

        public static UniTask<UnityEngine.Object> ToUniTask (this AssetBundleRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<UnityEngine.Object>(new AssetBundleRequestAwaiter(resourceRequest));
        }

        public static UniTask<UnityEngine.Object> ConfigureAwait (this AssetBundleRequest resourceRequest, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new AssetBundleRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(timing, awaiter);
            }
            return new UniTask<UnityEngine.Object>(awaiter);
        }

        public static AssetBundleCreateRequestAwaiter GetAwaiter (this AssetBundleCreateRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new AssetBundleCreateRequestAwaiter(resourceRequest);
        }

        public static UniTask<AssetBundle> ToUniTask (this AssetBundleCreateRequest resourceRequest)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));
            return new UniTask<AssetBundle>(new AssetBundleCreateRequestAwaiter(resourceRequest));
        }

        public static UniTask<AssetBundle> ConfigureAwait (this AssetBundleCreateRequest resourceRequest, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(resourceRequest, nameof(resourceRequest));

            var awaiter = new AssetBundleCreateRequestConfiguredAwaiter(resourceRequest, progress, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(timing, awaiter);
            }
            return new UniTask<AssetBundle>(awaiter);
        }

        #if ENABLE_UNITYWEBREQUEST

        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter (this UnityWebRequestAsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }

        public static UniTask<UnityWebRequest> ToUniTask (this UnityWebRequestAsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new UniTask<UnityWebRequest>(new UnityWebRequestAsyncOperationAwaiter(asyncOperation));
        }

        public static UniTask<UnityWebRequest> ConfigureAwait (this UnityWebRequestAsyncOperation asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellation = default)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));

            var awaiter = new UnityWebRequestAsyncOperationConfiguredAwaiter(asyncOperation, progress, cancellation);
            if (!awaiter.IsCompleted)
            {
                PlayerLoopHelper.AddAction(timing, awaiter);
            }
            return new UniTask<UnityWebRequest>(awaiter);
        }

        #endif

        public struct AsyncOperationAwaiter : IAwaiter
        {
            private AsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AwaiterStatus status;

            public AsyncOperationAwaiter (AsyncOperation asyncOperation)
            {
                this.status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = this.status.IsCompleted() ? null : asyncOperation;
                this.continuationAction = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public void GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return;

                if (status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                    {
                        status = AwaiterStatus.Succeeded;
                    }
                    else
                    {
                        Error.ThrowNotYetCompleted();
                    }
                }

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }
            }

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AsyncOperationConfiguredAwaiter : IAwaiter, IPlayerLoopItem
        {
            private AsyncOperation asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private AwaiterStatus status;
            private Action continuation;

            public AsyncOperationConfiguredAwaiter (AsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                this.status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (this.status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                this.continuation = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public void GetResult ()
            {
                if (status == AwaiterStatus.Succeeded)
                {
                    return;
                }
                else if (status == AwaiterStatus.Canceled)
                {
                    Error.ThrowOperationCanceledException();
                }

                Error.ThrowNotYetCompleted();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(asyncOperation.progress);
                }

                if (asyncOperation.isDone)
                {
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                this.status = status;
                var cont = this.continuation;

                // cleanup
                this.continuation = null;
                this.cancellationToken = CancellationToken.None;
                this.progress = null;
                this.asyncOperation = null;

                if (cont != null) cont.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        public struct ResourceRequestAwaiter : IAwaiter<UnityEngine.Object>
        {
            private ResourceRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AwaiterStatus status;
            private UnityEngine.Object result;

            public ResourceRequestAwaiter (ResourceRequest asyncOperation)
            {
                this.status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = this.status.IsCompleted() ? null : asyncOperation;
                this.result = this.status.IsCompletedSuccessfully() ? asyncOperation.asset : null;
                this.continuationAction = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public UnityEngine.Object GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                    {
                        status = AwaiterStatus.Succeeded;
                    }
                    else
                    {
                        Error.ThrowNotYetCompleted();
                    }
                }

                this.result = asyncOperation.asset;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return this.result;
            }

            void IAwaiter.GetResult () => GetResult();

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class ResourceRequestConfiguredAwaiter : IAwaiter<UnityEngine.Object>, IPlayerLoopItem
        {
            private ResourceRequest asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private AwaiterStatus status;
            private Action continuation;
            private UnityEngine.Object result;

            public ResourceRequestConfiguredAwaiter (ResourceRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                this.status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (this.status.IsCompletedSuccessfully()) this.result = asyncOperation.asset;
                if (this.status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                this.continuation = null;
                this.result = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;
            void IAwaiter.GetResult () => GetResult();

            public UnityEngine.Object GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Canceled)
                {
                    Error.ThrowOperationCanceledException();
                }

                return Error.ThrowNotYetCompleted<UnityEngine.Object>();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(asyncOperation.progress);
                }

                if (asyncOperation.isDone)
                {
                    this.result = asyncOperation.asset;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                this.status = status;
                var cont = this.continuation;

                // cleanup
                this.continuation = null;
                this.cancellationToken = CancellationToken.None;
                this.progress = null;
                this.asyncOperation = null;

                if (cont != null) cont.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        public struct AssetBundleRequestAwaiter : IAwaiter<UnityEngine.Object>
        {
            private AssetBundleRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AwaiterStatus status;
            private UnityEngine.Object result;

            public AssetBundleRequestAwaiter (AssetBundleRequest asyncOperation)
            {
                this.status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = this.status.IsCompleted() ? null : asyncOperation;
                this.result = this.status.IsCompletedSuccessfully() ? asyncOperation.asset : null;
                this.continuationAction = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public UnityEngine.Object GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                    {
                        status = AwaiterStatus.Succeeded;
                    }
                    else
                    {
                        Error.ThrowNotYetCompleted();
                    }
                }

                this.result = asyncOperation.asset;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return this.result;
            }

            void IAwaiter.GetResult () => GetResult();

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AssetBundleRequestConfiguredAwaiter : IAwaiter<UnityEngine.Object>, IPlayerLoopItem
        {
            private AssetBundleRequest asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private AwaiterStatus status;
            private Action continuation;
            private UnityEngine.Object result;

            public AssetBundleRequestConfiguredAwaiter (AssetBundleRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                this.status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (this.status.IsCompletedSuccessfully()) this.result = asyncOperation.asset;
                if (this.status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                this.continuation = null;
                this.result = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;
            void IAwaiter.GetResult () => GetResult();

            public UnityEngine.Object GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Canceled)
                {
                    Error.ThrowOperationCanceledException();
                }

                return Error.ThrowNotYetCompleted<UnityEngine.Object>();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(asyncOperation.progress);
                }

                if (asyncOperation.isDone)
                {
                    this.result = asyncOperation.asset;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                this.status = status;
                var cont = this.continuation;

                // cleanup
                this.continuation = null;
                this.cancellationToken = CancellationToken.None;
                this.progress = null;
                this.asyncOperation = null;

                if (cont != null) cont.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        public struct AssetBundleCreateRequestAwaiter : IAwaiter<AssetBundle>
        {
            private AssetBundleCreateRequest asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AwaiterStatus status;
            private AssetBundle result;

            public AssetBundleCreateRequestAwaiter (AssetBundleCreateRequest asyncOperation)
            {
                this.status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = this.status.IsCompleted() ? null : asyncOperation;
                this.result = this.status.IsCompletedSuccessfully() ? asyncOperation.assetBundle : null;
                this.continuationAction = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public AssetBundle GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                    {
                        status = AwaiterStatus.Succeeded;
                    }
                    else
                    {
                        Error.ThrowNotYetCompleted();
                    }
                }

                this.result = asyncOperation.assetBundle;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return this.result;
            }

            void IAwaiter.GetResult () => GetResult();

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class AssetBundleCreateRequestConfiguredAwaiter : IAwaiter<AssetBundle>, IPlayerLoopItem
        {
            private AssetBundleCreateRequest asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private AwaiterStatus status;
            private Action continuation;
            private AssetBundle result;

            public AssetBundleCreateRequestConfiguredAwaiter (AssetBundleCreateRequest asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                this.status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (this.status.IsCompletedSuccessfully()) this.result = asyncOperation.assetBundle;
                if (this.status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                this.continuation = null;
                this.result = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;
            void IAwaiter.GetResult () => GetResult();

            public AssetBundle GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Canceled)
                {
                    Error.ThrowOperationCanceledException();
                }

                return Error.ThrowNotYetCompleted<AssetBundle>();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(asyncOperation.progress);
                }

                if (asyncOperation.isDone)
                {
                    this.result = asyncOperation.assetBundle;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                this.status = status;
                var cont = this.continuation;

                // cleanup
                this.continuation = null;
                this.cancellationToken = CancellationToken.None;
                this.progress = null;
                this.asyncOperation = null;

                if (cont != null) cont.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        #if ENABLE_UNITYWEBREQUEST

        public struct UnityWebRequestAsyncOperationAwaiter : IAwaiter<UnityWebRequest>
        {
            private UnityWebRequestAsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;
            private AwaiterStatus status;
            private UnityWebRequest result;

            public UnityWebRequestAsyncOperationAwaiter (UnityWebRequestAsyncOperation asyncOperation)
            {
                this.status = asyncOperation.isDone ? AwaiterStatus.Succeeded : AwaiterStatus.Pending;
                this.asyncOperation = this.status.IsCompleted() ? null : asyncOperation;
                this.result = this.status.IsCompletedSuccessfully() ? asyncOperation.webRequest : null;
                this.continuationAction = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;

            public UnityWebRequest GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Pending)
                {
                    // first timing of call
                    if (asyncOperation.isDone)
                    {
                        status = AwaiterStatus.Succeeded;
                    }
                    else
                    {
                        Error.ThrowNotYetCompleted();
                    }
                }

                this.result = asyncOperation.webRequest;

                if (continuationAction != null)
                {
                    asyncOperation.completed -= continuationAction;
                    asyncOperation = null; // remove reference.
                    continuationAction = null;
                }
                else
                {
                    asyncOperation = null; // remove reference.
                }

                return this.result;
            }

            void IAwaiter.GetResult () => GetResult();

            public void OnCompleted (Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = continuation.AsFuncOfT<AsyncOperation>();
                asyncOperation.completed += continuationAction;
            }
        }

        private class UnityWebRequestAsyncOperationConfiguredAwaiter : IAwaiter<UnityWebRequest>, IPlayerLoopItem
        {
            private UnityWebRequestAsyncOperation asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;
            private AwaiterStatus status;
            private Action continuation;
            private UnityWebRequest result;

            public UnityWebRequestAsyncOperationConfiguredAwaiter (UnityWebRequestAsyncOperation asyncOperation, IProgress<float> progress, CancellationToken cancellationToken)
            {
                this.status = cancellationToken.IsCancellationRequested ? AwaiterStatus.Canceled
                    : asyncOperation.isDone ? AwaiterStatus.Succeeded
                    : AwaiterStatus.Pending;

                if (this.status.IsCompletedSuccessfully()) this.result = asyncOperation.webRequest;
                if (this.status.IsCompleted()) return;

                this.asyncOperation = asyncOperation;
                this.progress = progress;
                this.cancellationToken = cancellationToken;
                this.continuation = null;
                this.result = null;
            }

            public bool IsCompleted => status.IsCompleted();
            public AwaiterStatus Status => status;
            void IAwaiter.GetResult () => GetResult();

            public UnityWebRequest GetResult ()
            {
                if (status == AwaiterStatus.Succeeded) return this.result;

                if (status == AwaiterStatus.Canceled)
                {
                    Error.ThrowOperationCanceledException();
                }

                return Error.ThrowNotYetCompleted<UnityWebRequest>();
            }

            public bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    InvokeContinuation(AwaiterStatus.Canceled);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report(asyncOperation.progress);
                }

                if (asyncOperation.isDone)
                {
                    this.result = asyncOperation.webRequest;
                    InvokeContinuation(AwaiterStatus.Succeeded);
                    return false;
                }

                return true;
            }

            private void InvokeContinuation (AwaiterStatus status)
            {
                this.status = status;
                var cont = this.continuation;

                // cleanup
                this.continuation = null;
                this.cancellationToken = CancellationToken.None;
                this.progress = null;
                this.asyncOperation = null;

                if (cont != null) cont.Invoke();
            }

            public void OnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuation);
                this.continuation = continuation;
            }
        }

        #endif
    }
}
