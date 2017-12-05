using System;
using UnityEngine;

/// <summary>
/// Allows tweening a <see cref="ITweenValue"/> using coroutine.
/// </summary>
public class Tweener<TTweenValue> : AsyncRunner where TTweenValue : struct, ITweenValue
{
    private float elapsedTime;
    private TTweenValue tweenValue;

    public Tweener (MonoBehaviour coroutineContainer = null,
        Action onCompleted = null) : base(coroutineContainer, onCompleted) { }

    public Tweener (TTweenValue tweenValue, MonoBehaviour coroutineContainer = null, 
        Action onCompleted = null) : base(coroutineContainer, onCompleted)
    {
        this.tweenValue = tweenValue;
    }

    public override void Run ()
    {
        Run(tweenValue);
    }

    public Tweener<TTweenValue> Run (TTweenValue tweenValue)
    {
        elapsedTime = 0f;
        this.tweenValue = tweenValue;
        Run();
        return this;
    }

    protected override bool LoopCondition ()
    {
        return elapsedTime < tweenValue.TweenDuration;
    }

    protected override void OnCoroutineTick ()
    {
        base.OnCoroutineTick();

        elapsedTime += tweenValue.IsTimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
        var tweenPercent = Mathf.Clamp01(elapsedTime / tweenValue.TweenDuration);
        tweenValue.TweenValue(tweenPercent);
    }

    public override void CompleteInstantly ()
    {
        tweenValue.TweenValue(1f);
        base.CompleteInstantly();
    }
}
