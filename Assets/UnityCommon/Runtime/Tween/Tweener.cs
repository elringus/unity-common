using System;
using UnityEngine;

/// <summary>
/// Allows tweening a <see cref="ITweenValue"/> using coroutine.
/// </summary>
public class Tweener<TTweenValue> : AsyncRunner<TTweenValue> where TTweenValue : struct, ITweenValue
{
    public TTweenValue TweenValue { get { return Result; } private set { Result = value; } }

    private float elapsedTime;

    public Tweener (MonoBehaviour coroutineContainer = null,
        Action onCompleted = null) : base(coroutineContainer, onCompleted) { }

    public Tweener (TTweenValue tweenValue, MonoBehaviour coroutineContainer = null, 
        Action onCompleted = null) : base(coroutineContainer, onCompleted)
    {
        TweenValue = tweenValue;
    }

    public Tweener<TTweenValue> Run (TTweenValue tweenValue)
    {
        TweenValue = tweenValue;
        return Run() as Tweener<TTweenValue>;
    }

    public override AsyncRunner<TTweenValue> Run ()
    {
        elapsedTime = 0f;

        if (TweenValue.TweenDuration <= 0f)
        {
            CompleteInstantly();
            return this;
        }

        return base.Run();
    }

    protected override bool LoopCondition ()
    {
        return elapsedTime <= TweenValue.TweenDuration;
    }

    protected override void OnCoroutineTick ()
    {
        base.OnCoroutineTick();

        elapsedTime += TweenValue.IsTimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
        var tweenPercent = Mathf.Clamp01(elapsedTime / TweenValue.TweenDuration);
        TweenValue.TweenValue(tweenPercent);
    }

    public override void CompleteInstantly ()
    {
        TweenValue.TweenValue(1f);
        base.CompleteInstantly();
    }
}
