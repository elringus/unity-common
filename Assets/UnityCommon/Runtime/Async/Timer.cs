using System;
using UnityEngine;

public class Timer : AsyncRunner
{
    public override bool CanBeInstantlyCompleted { get { return true; } }
    public bool IsTimeScaleIgnored { get; private set; }
    public float Duration { get; private set; }
    public float ElapsedTime { get; private set; }

    public Timer (float duration = 0f, bool ignoreTimeScale = false, 
        MonoBehaviour coroutineContainer = null, Action onCompleted = null) : base(coroutineContainer, onCompleted)
    {
        Duration = duration;
        IsTimeScaleIgnored = ignoreTimeScale;
    }

    public Timer Run (float duration, bool ignoreTimeScale = false)
    {
        ElapsedTime = 0f;
        Duration = duration;
        IsTimeScaleIgnored = ignoreTimeScale;

        base.Run();

        return this;
    }

    protected override bool LoopCondition ()
    {
        return ElapsedTime < Duration;
    }

    protected override void OnCoroutineTick ()
    {
        base.OnCoroutineTick();

        ElapsedTime += IsTimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
