using UnityEngine;
using UnityEngine.Events;

public class Timer : AsyncRunner
{
    public override bool CanBeInstantlyCompleted { get { return true; } }
    public bool IsTimeScaleIgnored { get; private set; }
    public float Duration { get; private set; }
    public float ElapsedTime { get; private set; }

    public Timer (MonoBehaviour coroutineContainer = null, UnityAction onComplete = null) :
        base(coroutineContainer, onComplete) { }

    public Timer Run (float duration, bool ignoreTimeScale = false)
    {
        ElapsedTime = 0f;
        Duration = duration;
        IsTimeScaleIgnored = ignoreTimeScale;

        StartRunner();

        return this;
    }

    protected override bool LoopCondition ()
    {
        return ElapsedTime < Duration;
    }

    protected override void OnRoutineTick ()
    {
        base.OnRoutineTick();

        ElapsedTime += IsTimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
