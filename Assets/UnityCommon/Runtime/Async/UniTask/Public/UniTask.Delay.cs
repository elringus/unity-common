using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityCommon.Async;
using UnityCommon.Async.Internal;
using UnityEngine;

namespace UnityCommon
{
    public readonly partial struct UniTask
    {
        public static YieldAwaitable Yield (PlayerLoopTiming timing = PlayerLoopTiming.Update, AsyncToken asyncToken = default)
        {
            return new YieldAwaitable(timing, asyncToken);
        }

        public static UniTask<int> DelayFrame (int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            if (delayFrameCount < 0)
            {
                throw new ArgumentOutOfRangeException("Delay does not allow minus delayFrameCount. delayFrameCount:" + delayFrameCount);
            }

            var source = new DelayFramePromise(delayFrameCount, delayTiming, cancellationToken);
            return source.Task;
        }

        public static UniTask Delay (int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            var delayTimeSpan = TimeSpan.FromMilliseconds(millisecondsDelay);
            if (delayTimeSpan < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("Delay does not allow minus delayFrameCount. delayTimeSpan:" + delayTimeSpan);
            }

            return ignoreTimeScale
                ? new DelayIgnoreTimeScalePromise(delayTimeSpan, delayTiming, cancellationToken).Task
                : new DelayPromise(delayTimeSpan, delayTiming, cancellationToken).Task;
        }

        public static UniTask Delay (TimeSpan delayTimeSpan, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            if (delayTimeSpan < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("Delay does not allow minus delayFrameCount. delayTimeSpan:" + delayTimeSpan);
            }

            return ignoreTimeScale
                ? new DelayIgnoreTimeScalePromise(delayTimeSpan, delayTiming, cancellationToken).Task
                : new DelayPromise(delayTimeSpan, delayTiming, cancellationToken).Task;
        }

        private class DelayFramePromise : PlayerLoopReusablePromiseBase<int>
        {
            private readonly int delayFrameCount;
            private int currentFrameCount;

            public DelayFramePromise (int delayFrameCount, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 2)
            {
                this.delayFrameCount = delayFrameCount;
                this.currentFrameCount = 0;
            }

            protected override void OnRunningStart ()
            {
                currentFrameCount = 0;
            }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                if (currentFrameCount == delayFrameCount)
                {
                    Complete();
                    TrySetResult(currentFrameCount);
                    return false;
                }

                currentFrameCount++;
                return true;
            }
        }

        private class DelayPromise : PlayerLoopReusablePromiseBase
        {
            private readonly float delayFrameTimeSpan;
            private float elapsed;

            public DelayPromise (TimeSpan delayFrameTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 2)
            {
                this.delayFrameTimeSpan = (float)delayFrameTimeSpan.TotalSeconds;
            }

            protected override void OnRunningStart ()
            {
                this.elapsed = 0.0f;
            }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                elapsed += Time.deltaTime;
                if (elapsed >= delayFrameTimeSpan)
                {
                    Complete();
                    TrySetResult();
                    return false;
                }

                return true;
            }
        }

        private class DelayIgnoreTimeScalePromise : PlayerLoopReusablePromiseBase
        {
            private readonly float delayFrameTimeSpan;
            private float elapsed;

            public DelayIgnoreTimeScalePromise (TimeSpan delayFrameTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 2)
            {
                this.delayFrameTimeSpan = (float)delayFrameTimeSpan.TotalSeconds;
            }

            protected override void OnRunningStart ()
            {
                this.elapsed = 0.0f;
            }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                elapsed += Time.unscaledDeltaTime;

                if (elapsed >= delayFrameTimeSpan)
                {
                    Complete();
                    TrySetResult();
                    return false;
                }

                return true;
            }
        }
    }

    public readonly struct YieldAwaitable
    {
        private readonly PlayerLoopTiming timing;
        private readonly AsyncToken token;

        public YieldAwaitable (PlayerLoopTiming timing, AsyncToken token = default)
        {
            this.timing = timing;
            this.token = token;
        }

        public Awaiter GetAwaiter ()
        {
            return new Awaiter(timing, token);
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => false;

            private readonly PlayerLoopTiming timing;
            private readonly AsyncToken token;

            public Awaiter (PlayerLoopTiming timing, AsyncToken token = default)
            {
                this.timing = timing;
                this.token = token;
            }

            public void GetResult ()
            {
                token.ThrowIfCanceled();
            }

            public void OnCompleted (Action continuation)
            {
                token.ThrowIfCanceled();
                PlayerLoopHelper.AddContinuation(timing, continuation);
            }

            public void UnsafeOnCompleted (Action continuation)
            {
                token.ThrowIfCanceled();
                PlayerLoopHelper.AddContinuation(timing, continuation);
            }
        }
    }
}
