using UnityEngine;
using UnityEngine.Events;

public class Tweener<T> : AsyncRunner where T : struct, ITweenValue
{
    public override bool CanBeInstantlyCompleted { get { return true; } }

    private float elapsedTime;
    private T tweenValue;

    public Tweener (MonoBehaviour coroutineContainer = null, UnityAction onComplete = null) :
        base(coroutineContainer, onComplete)
    {
        
    }

    public Tweener<T> Run (T tweenValue)
    {
        elapsedTime = 0f;
        this.tweenValue = tweenValue;

        StartRunner();

        return this;
    }

    protected override bool LoopCondition ()
    {
        return elapsedTime < tweenValue.TweenDuration;
    }

    protected override void OnRoutineTick ()
    {
        base.OnRoutineTick();

        elapsedTime += tweenValue.IsTimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
        var tweenPercent = Mathf.Clamp01(elapsedTime / tweenValue.TweenDuration);
        tweenValue.TweenValue(tweenPercent);
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        tweenValue.TweenValue(1f);
    }
}
