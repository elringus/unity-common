using System;
using UniRx.Async;

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
            CancellationToken cancellationToken = default, UnityEngine.Object target = default)
        {
            if (Running) CompleteInstantly();

            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;
            Running = true;

            targetProvided = this.target = target;

            if (Loop) WaitAndLoop(cancellationToken).Forget();
            else WaitAndComplete(cancellationToken).Forget();
        }

        public void Run (CancellationToken cancellationToken = default, UnityEngine.Object target = default) 
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

        protected virtual async UniTaskVoid WaitAndComplete (CancellationToken cancellationToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;

            using (var combinedCTS = cancellationToken.CreateLinkedTokenSource())
                await UniTask.Delay(TimeSpan.FromSeconds(Duration), TimeScaleIgnored, cancellationToken: combinedCTS.Token);
            if (cancellationToken.CancelASAP || !TargetValid) return;
            if (lastRunGuid != currentRunGuid) return; // The timer was completed instantly or stopped.

            if (cancellationToken.CancelLazy) CompleteInstantly();
            else
            {
                Running = false;
                onCompleted?.Invoke();
            }
        }

        protected virtual async UniTaskVoid WaitAndLoop (CancellationToken cancellationToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;

            while (!cancellationToken.CancellationRequested)
            {
                using (var combinedCTS = cancellationToken.CreateLinkedTokenSource())
                    await UniTask.Delay(TimeSpan.FromSeconds(Duration), TimeScaleIgnored, cancellationToken: combinedCTS.Token);
                if (cancellationToken.CancelASAP || !TargetValid) return;
                if (lastRunGuid != currentRunGuid) return; // The timer was stopped.
                onLoop?.Invoke();
            }

            if (cancellationToken.CancelLazy) CompleteInstantly();
        }
    }
}
