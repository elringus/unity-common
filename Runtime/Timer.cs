using System;
using System.Threading;
using UnityEngine;

namespace UnityCommon
{
    public class Timer : CoroutineRunner
    {
        public event Action OnLoop;

        public override bool CanInstantlyComplete => true;
        public bool Loop { get; private set; }
        public bool TimeScaleIgnored { get; private set; }
        public float Duration { get; private set; }
        public float ElapsedTime { get; private set; }

        public Timer (float duration = 0f, bool loop = false, bool ignoreTimeScale = false,
            MonoBehaviour coroutineContainer = null, Action onCompleted = null, Action onLoop = null) : base(coroutineContainer)
        {
            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;

            if (onCompleted != null) OnCompleted += onCompleted;
            if (onLoop != null) OnLoop += onLoop;
        }

        public void Run (float duration, bool loop = false, bool ignoreTimeScale = false, CancellationToken cancellationToken = default)
        {
            ElapsedTime = 0f;
            Duration = duration;
            Loop = loop;
            TimeScaleIgnored = ignoreTimeScale;

            base.Run(cancellationToken);
        }

        public override void Run (CancellationToken cancellationToken = default) => Run(Duration, Loop, TimeScaleIgnored, cancellationToken);

        public override void Stop ()
        {
            base.Stop();

            Loop = false;
        }

        protected override bool LoopCondition ()
        {
            return !CancellationToken.IsCancellationRequested && ElapsedTime < Duration;
        }

        protected override void OnCoroutineTick ()
        {
            base.OnCoroutineTick();

            ElapsedTime += TimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        protected override void HandleOnCompleted ()
        {
            if (Loop)
            {
                OnLoop.SafeInvoke();
                base.Stop();
                Run();
            }
            else base.HandleOnCompleted();
        }
    }
}
