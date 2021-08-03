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
            in AsyncToken asyncToken = default, UnityEngine.Object target = default)
        {
            if (Running) CompleteInstantly();

            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;
            Running = true;

            targetProvided = this.target = target;

            if (Loop) WaitAndLoopAsync(asyncToken).Forget();
            else WaitAndCompleteAsync(asyncToken).Forget();
        }

        public void Run (in AsyncToken asyncToken = default, UnityEngine.Object target = default) 
            => Run(Duration, Loop, TimeScaleIgnored, asyncToken, target);

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

        protected virtual async UniTaskVoid WaitAndCompleteAsync (AsyncToken asyncToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;
            var startTime = GetTime();

            while (!WaitedEnough(startTime) && !asyncToken.CanceledOrCompleted)
                await AsyncUtils.WaitEndOfFrame;
            
            if (asyncToken.Canceled || !TargetValid) return;
            if (lastRunGuid != currentRunGuid) return; // The timer was completed instantly or stopped.

            if (asyncToken.Completed) CompleteInstantly();
            else
            {
                Running = false;
                onCompleted?.Invoke();
            }
        }

        protected virtual async UniTaskVoid WaitAndLoopAsync (AsyncToken asyncToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;
            var startTime = GetTime();
            
            while (!asyncToken.CanceledOrCompleted)
            {
                await AsyncUtils.WaitEndOfFrame;
                if (asyncToken.Canceled || !TargetValid) return;
                if (lastRunGuid != currentRunGuid) return; // The timer was stopped.
                if (WaitedEnough(startTime))
                {
                    onLoop?.Invoke();
                    startTime = GetTime();
                }
            }

            if (asyncToken.Completed) CompleteInstantly();
        }

        private float GetTime () => TimeScaleIgnored ? Time.unscaledTime : Time.time;
        
        private bool WaitedEnough (float startTime) => GetTime() - startTime >= Duration;
    }
}
