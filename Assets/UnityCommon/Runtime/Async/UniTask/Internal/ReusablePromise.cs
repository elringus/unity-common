using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace UnityCommon.Async.Internal
{
    // public for some types uses it.

    public abstract class ReusablePromise : IAwaiter
    {
        private ExceptionDispatchInfo exception;
        private object continuation; // Action or Queue<Action>
        private AwaiterStatus status;

        public UniTask Task => new UniTask(this);

        // can override for control 'start/reset' timing.
        public virtual bool IsCompleted => status.IsCompleted();

        public virtual void GetResult ()
        {
            switch (status)
            {
                case AwaiterStatus.Succeeded:
                    return;
                case AwaiterStatus.Faulted:
                    exception.Throw();
                    break;
                case AwaiterStatus.Canceled:
                    throw new OperationCanceledException();
            }

            throw new InvalidOperationException("Invalid Status:" + status);
        }

        public AwaiterStatus Status => status;

        void IAwaiter.GetResult ()
        {
            GetResult();
        }

        public void ResetStatus (bool forceReset)
        {
            if (forceReset)
            {
                status = AwaiterStatus.Pending;
            }
            else if (status == AwaiterStatus.Succeeded)
            {
                status = AwaiterStatus.Pending;
            }
        }

        public virtual bool TrySetCanceled ()
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Canceled;
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public virtual bool TrySetException (Exception ex)
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Faulted;
                exception = ExceptionDispatchInfo.Capture(ex);
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public virtual bool TrySetResult ()
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Succeeded;
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        private void TryInvokeContinuation ()
        {
            if (continuation == null) return;

            if (continuation is Action act)
            {
                continuation = null;
                act();
            }
            else
            {
                // reuse Queue(don't null clear)
                var q = (MinimumQueue<Action>)continuation;
                var size = q.Count;
                for (int i = 0; i < size; i++)
                {
                    q.Dequeue().Invoke();
                }
            }
        }

        public void OnCompleted (Action action)
        {
            UnsafeOnCompleted(action);
        }

        public void UnsafeOnCompleted (Action action)
        {
            if (continuation == null)
            {
                continuation = action;
                return;
            }
            else
            {
                if (continuation is Action act)
                {
                    var q = new MinimumQueue<Action>(4);
                    q.Enqueue(act);
                    q.Enqueue(action);
                    continuation = q;
                    return;
                }
                else
                {
                    ((MinimumQueue<Action>)continuation).Enqueue(action);
                }
            }
        }
    }

    public abstract class ReusablePromise<T> : IAwaiter<T>
    {
        private T result;
        private ExceptionDispatchInfo exception;
        private object continuation; // Action or Queue<Action>
        private AwaiterStatus status;

        public UniTask<T> Task => new UniTask<T>(this);

        // can override for control 'start/reset' timing.
        public virtual bool IsCompleted => status.IsCompleted();

        protected T RawResult => result;

        protected void ForceSetResult (T result)
        {
            this.result = result;
        }

        public virtual T GetResult ()
        {
            switch (status)
            {
                case AwaiterStatus.Succeeded:
                    return result;
                case AwaiterStatus.Faulted:
                    exception.Throw();
                    break;
                case AwaiterStatus.Canceled:
                    throw new OperationCanceledException();
            }

            throw new InvalidOperationException("Invalid Status:" + status);
        }

        public AwaiterStatus Status => status;

        void IAwaiter.GetResult ()
        {
            GetResult();
        }

        public void ResetStatus (bool forceReset)
        {
            if (forceReset)
            {
                status = AwaiterStatus.Pending;
            }
            else if (status == AwaiterStatus.Succeeded)
            {
                status = AwaiterStatus.Pending;
            }
        }

        public virtual bool TrySetCanceled ()
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Canceled;
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public virtual bool TrySetException (Exception ex)
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Faulted;
                exception = ExceptionDispatchInfo.Capture(ex);
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        public virtual bool TrySetResult (T result)
        {
            if (status == AwaiterStatus.Pending)
            {
                status = AwaiterStatus.Succeeded;
                this.result = result;
                TryInvokeContinuation();
                return true;
            }
            return false;
        }

        protected void TryInvokeContinuation ()
        {
            if (continuation == null) return;

            if (continuation is Action act)
            {
                continuation = null;
                act();
            }
            else
            {
                // reuse Queue(don't null clear)
                var q = (MinimumQueue<Action>)continuation;
                var size = q.Count;
                for (int i = 0; i < size; i++)
                {
                    q.Dequeue().Invoke();
                }
            }
        }

        public void OnCompleted (Action action)
        {
            UnsafeOnCompleted(action);
        }

        public void UnsafeOnCompleted (Action action)
        {
            if (continuation == null)
            {
                continuation = action;
                return;
            }
            else
            {
                if (continuation is Action act)
                {
                    var q = new MinimumQueue<Action>(4);
                    q.Enqueue(act);
                    q.Enqueue(action);
                    continuation = q;
                    return;
                }
                else
                {
                    ((MinimumQueue<Action>)continuation).Enqueue(action);
                }
            }
        }
    }

    public abstract class PlayerLoopReusablePromiseBase : ReusablePromise, IPlayerLoopItem
    {
        private readonly PlayerLoopTiming timing;
        protected readonly CancellationToken cancellationToken;
        private bool isRunning = false;

        protected PlayerLoopReusablePromiseBase (PlayerLoopTiming timing, CancellationToken cancellationToken, int skipTrackFrameCountAdditive)
        {
            this.timing = timing;
            this.cancellationToken = cancellationToken;
        }

        public override bool IsCompleted
        {
            get
            {
                if (Status == AwaiterStatus.Canceled || Status == AwaiterStatus.Faulted) return true;

                if (!isRunning)
                {
                    isRunning = true;
                    ResetStatus(false);
                    OnRunningStart();
                    PlayerLoopHelper.AddAction(timing, this);
                }
                return false;
            }
        }

        protected abstract void OnRunningStart ();

        protected void Complete ()
        {
            isRunning = false;
        }

        public abstract bool MoveNext ();
    }

    public abstract class PlayerLoopReusablePromiseBase<T> : ReusablePromise<T>, IPlayerLoopItem
    {
        private readonly PlayerLoopTiming timing;
        protected readonly CancellationToken cancellationToken;
        private bool isRunning = false;

        protected PlayerLoopReusablePromiseBase (PlayerLoopTiming timing, CancellationToken cancellationToken, int skipTrackFrameCountAdditive)
        {
            this.timing = timing;
            this.cancellationToken = cancellationToken;
        }

        public override bool IsCompleted
        {
            get
            {
                if (Status == AwaiterStatus.Canceled || Status == AwaiterStatus.Faulted) return true;

                if (!isRunning)
                {
                    isRunning = true;
                    ResetStatus(false);
                    OnRunningStart();
                    PlayerLoopHelper.AddAction(timing, this);
                }
                return false;
            }
        }

        protected abstract void OnRunningStart ();

        protected void Complete ()
        {
            isRunning = false;
        }

        public abstract bool MoveNext ();
    }
}
