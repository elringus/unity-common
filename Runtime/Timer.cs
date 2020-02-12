using System;
using System.Threading;
using UniRx.Async;

namespace UnityCommon
{
    public class Timer
    {
        public bool Running { get; private set; }
        public bool Loop { get; private set; }
        public bool TimeScaleIgnored { get; private set; }
        public float Duration { get; private set; }

        private readonly Action onLoop;
        private readonly Action onCompleted;
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

        public void Run (float duration, bool loop = false, bool ignoreTimeScale = false, CancellationToken cancellationToken = default)
        {
            if (Running) CompleteInstantly();

            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;
            Running = true;

            if (Loop) WaitAndLoop(cancellationToken).Forget();
            else WaitAndComplete(cancellationToken).Forget();
        }

        public void Run (CancellationToken cancellationToken = default) => Run(Duration, Loop, TimeScaleIgnored, cancellationToken);

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

            await UniTask.Delay(TimeSpan.FromSeconds(Duration), TimeScaleIgnored, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            if (lastRunGuid != currentRunGuid) return; // The timer was completed instantly or stopped.

            Running = false;
            onCompleted?.Invoke();
        }

        protected virtual async UniTaskVoid WaitAndLoop (CancellationToken cancellationToken = default)
        {
            lastRunGuid = Guid.NewGuid();
            var currentRunGuid = lastRunGuid;

            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Duration), TimeScaleIgnored, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
                if (lastRunGuid != currentRunGuid) return; // The timer was stopped.
                onLoop?.Invoke();
            }
        }
    }
}
