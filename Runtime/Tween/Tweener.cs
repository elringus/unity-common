using System;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows tweening a <see cref="ITweenValue"/> using coroutine.
    /// </summary>
    public interface ITweener<TTweenValue>
        where TTweenValue : struct, ITweenValue
    {
        TTweenValue TweenValue { get; }
        bool Running { get; }

        void Run (in TTweenValue tweenValue, in CancellationToken cancellationToken = default);
        void Run (in CancellationToken cancellationToken = default);
        UniTask RunAsync (in TTweenValue tweenValue, in CancellationToken cancellationToken = default);
        UniTask RunAsync (in CancellationToken cancellationToken = default);
        void Stop ();
        void CompleteInstantly ();
    }

    /// <inheritdoc cref="ITweener{TTweenValue}"/>
    public class Tweener<TTweenValue> : ITweener<TTweenValue>
        where TTweenValue : struct, ITweenValue
    {
        public TTweenValue TweenValue { get; private set; }
        public bool Running { get; private set; }

        private readonly Action onCompleted;
        private float elapsedTime;
        private Guid lastRunGuid;

        public Tweener (Action onCompleted = null)
        {
            this.onCompleted = onCompleted;
        }

        public Tweener (in TTweenValue tweenValue, Action onCompleted = null)
            : this(onCompleted)
        {
            TweenValue = tweenValue;
        }

        public void Run (in TTweenValue tweenValue, in CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            Run(cancellationToken);
        }

        public void Run (in CancellationToken cancellationToken = default) => TweenAsyncAndForget(cancellationToken).Forget();

        public UniTask RunAsync (in TTweenValue tweenValue, in CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            return RunAsync(cancellationToken);
        }

        public UniTask RunAsync (in CancellationToken cancellationToken = default) => TweenAsync(cancellationToken);

        public void Stop ()
        {
            lastRunGuid = Guid.Empty;
            Running = false;
        }

        public void CompleteInstantly ()
        {
            Stop();
            TweenValue.TweenValue(1f);
            onCompleted?.Invoke();
        }

        protected async UniTask TweenAsync (CancellationToken cancellationToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!cancellationToken.CancellationRequested && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PerformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (cancellationToken.CancelASAP) return;
            if (cancellationToken.CancelLazy) CompleteInstantly();
            else FinishTween();
        }

        // Required to prevent garbage when await is not required (fire and forget).
        // Remember to keep both methods identical.
        protected async UniTaskVoid TweenAsyncAndForget (CancellationToken cancellationToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!cancellationToken.CancellationRequested && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PerformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (cancellationToken.CancelASAP) return;
            if (cancellationToken.CancelLazy) CompleteInstantly();
            else FinishTween();
        }

        private void PrepareTween ()
        {
            if (Running) CompleteInstantly();

            Running = true;
            elapsedTime = 0f;
            lastRunGuid = Guid.NewGuid();
        }

        private void PerformTween ()
        {
            elapsedTime += TweenValue.TimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
            var tweenPercent = Mathf.Clamp01(elapsedTime / TweenValue.TweenDuration);
            TweenValue.TweenValue(tweenPercent);
        }

        private void FinishTween ()
        {
            Running = false;
            onCompleted?.Invoke();
        }
    }
}
