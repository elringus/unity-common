using System;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    public class Timer
    {
        public bool Running { get; private set; }
        public bool Loop { get; private set; }
        public bool TimeScaleIgnored { get; private set; }
        public float Duration { get; private set; }
        public bool TargetValid => !targetProvided || target;

        private readonly Action onLoop;
        private readonly Action onCompleted;
        private UnityEngine.Object target;
        private bool targetProvided;
        private Guid lastRunGuid;

        public Timer (float duration = 0f, bool loop = false, bool ignoreTimeScale = false,
            Action onCompleted = null, Action onLoop = null)
        {
            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;

            this.onLoop += onLoop;
            this.onCompleted += onCompleted;
        }

        public void Run (float duration, bool loop = false, bool ignoreTimeScale = false, 
            in CancellationToken cancellationToken = default, UnityEngine.Object target = default)
        {
            if (Running) CompleteInstantly();

            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;
            Running = true;

            targetProvided = this.target = target;

            if (Loop) WaitAndLoopAsync(cancellationToken).Forget();
            else WaitAndCompleteAsync(cancellationToken).Forget();
        }

        public void Run (in CancellationToken cancellationToken = default, UnityEngine.Object target = default) 
            => Run(Duration, Loop, TimeScaleIgnored, cancellationToken, target);

        public void Stop ()
        {
            lastRunGuid = Guid.Empty;
            Running = false;
        }

        public void CompleteInstantly ()
        {
            Stop();
            onCompleted?.Invoke();
        }

        protected virtual async UniTaskVoid WaitAndCompleteAsync (CancellationToken cancellationToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;
            var startTime = GetTime();

            while (!WaitedEnough(startTime) && !cancellationToken.CancellationRequested)
                await AsyncUtils.WaitEndOfFrame;
            
            if (cancellationToken.CancelASAP || !TargetValid) return;
            if (lastRunGuid != currentRunGuid) return; // The timer was completed instantly or stopped.

            if (cancellationToken.CancelLazy) CompleteInstantly();
            else
            {
                Running = false;
                onCompleted?.Invoke();
            }
        }

        protected virtual async UniTaskVoid WaitAndLoopAsync (CancellationToken cancellationToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;
            var startTime = GetTime();
            
            while (!cancellationToken.CancellationRequested)
            {
                await AsyncUtils.WaitEndOfFrame;
                if (cancellationToken.CancelASAP || !TargetValid) return;
                if (lastRunGuid != currentRunGuid) return; // The timer was stopped.
                if (WaitedEnough(startTime))
                {
                    onLoop?.Invoke();
                    startTime = GetTime();
                }
            }

            if (cancellationToken.CancelLazy) CompleteInstantly();
        }

        private float GetTime () => TimeScaleIgnored ? Time.unscaledTime : Time.time;
        
        private bool WaitedEnough (float startTime) => GetTime() - startTime >= Duration;
    }
}
