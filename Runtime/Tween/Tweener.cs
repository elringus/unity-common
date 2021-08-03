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

        void Run (in TTweenValue tweenValue, in AsyncToken asyncToken = default);
        void Run (in AsyncToken asyncToken = default);
        UniTask RunAsync (in TTweenValue tweenValue, in AsyncToken asyncToken = default);
        UniTask RunAsync (in AsyncToken asyncToken = default);
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

        public void Run (in TTweenValue tweenValue, in AsyncToken asyncToken = default)
        {
            TweenValue = tweenValue;
            Run(asyncToken);
        }

        public void Run (in AsyncToken asyncToken = default) => TweenAsyncAndForget(asyncToken).Forget();

        public UniTask RunAsync (in TTweenValue tweenValue, in AsyncToken asyncToken = default)
        {
            TweenValue = tweenValue;
            return RunAsync(asyncToken);
        }

        public UniTask RunAsync (in AsyncToken asyncToken = default) => TweenAsync(asyncToken);

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
        
        protected async UniTask TweenAsync (AsyncToken asyncToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!asyncToken.CanceledOrCompleted && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PerformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (asyncToken.Canceled) return;
            if (asyncToken.Completed) CompleteInstantly();
            else FinishTween();
        }

        // Required to prevent garbage when await is not required (fire and forget).
        // Remember to keep both methods identical.
        protected async UniTaskVoid TweenAsyncAndForget (AsyncToken asyncToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!asyncToken.CanceledOrCompleted && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PerformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (asyncToken.Canceled) return;
            if (asyncToken.Completed) CompleteInstantly();
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
