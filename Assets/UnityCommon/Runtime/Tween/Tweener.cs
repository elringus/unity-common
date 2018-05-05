using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Allows tweening a <see cref="ITweenValue"/> using coroutine.
/// </summary>
public class Tweener<TTweenValue> : CoroutineRunner where TTweenValue : struct, ITweenValue
{
    public TTweenValue TweenValue { get; private set; }

    private float elapsedTime;

    public Tweener (MonoBehaviour coroutineContainer = null,
        Action onCompleted = null) : base(coroutineContainer)
    {
        if (onCompleted != null) OnCompleted += onCompleted;
    }

    public Tweener (TTweenValue tweenValue, MonoBehaviour coroutineContainer = null, 
        Action onCompleted = null) : this(coroutineContainer, onCompleted)
    {
        TweenValue = tweenValue;
    }

    public void Run (TTweenValue tweenValue)
    {
        TweenValue = tweenValue;
        Run();
    }

    public async Task RunAsync (TTweenValue tweenValue)
    {
        Run(tweenValue);
        var taskCompletionSource = new TaskCompletionSource<object>();
        if (IsCompleted) taskCompletionSource.SetResult(null);
        else OnCompleted += () => taskCompletionSource.SetResult(null);
        await taskCompletionSource.Task;
    }

    public override void Run ()
    {
        elapsedTime = 0f;

        if (TweenValue.TweenDuration <= 0f)
        {
            CompleteInstantly();
            return;
        }

        base.Run();
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
