using System;
using System.Threading;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows tweening a <see cref="ITweenValue"/> using coroutine.
    /// </summary>
    public class Tweener<TTweenValue> 
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

        public Tweener (TTweenValue tweenValue, Action onCompleted = null) 
            : this(onCompleted)
        {
            TweenValue = tweenValue;
        }

        public void Run (TTweenValue tweenValue, CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            Run(cancellationToken);
        }

        public void Run (CancellationToken cancellationToken = default) => TweenAsync(cancellationToken).Forget();

        public UniTask RunAsync (TTweenValue tweenValue, CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            return RunAsync(cancellationToken);
        }

        public async UniTask RunAsync (CancellationToken cancellationToken = default) => await TweenAsync(cancellationToken);

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

        protected async UniTaskVoid TweenAsync (CancellationToken cancellationToken = default)
        {
            if (Running) CompleteInstantly();

            Running = true;
            lastRunGuid = new Guid();
            var currentRunGuid = lastRunGuid;
            elapsedTime = 0f;

            if (TweenValue.TweenDuration <= 0f)
            {
                CompleteInstantly();
                return;
            }

            while (!cancellationToken.IsCancellationRequested && elapsedTime <= TweenValue.TweenDuration)
            {
                elapsedTime += TweenValue.TimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
                var tweenPercent = Mathf.Clamp01(elapsedTime / TweenValue.TweenDuration);
                TweenValue.TweenValue(tweenPercent);
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            Running = false;
            onCompleted?.Invoke();
        }
    }
}
