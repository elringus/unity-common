using UnityEngine;
using UnityEngine.Events;

public struct FloatTween : ITweenValue
{
    class OnFloatTween : UnityEvent<float> { }

    public float StartValue { get; set; }
    public float TargetValue { get; set; }
    public float TweenDuration { get; set; }
    public bool IsTimeScaleIgnored { get; set; }
    public bool IsTargetValid { get { return onFloatTween != null; } }

    private OnFloatTween onFloatTween;

    public FloatTween (float from, float to, float time, UnityAction<float> onTween, bool ignoreTimeScale = false)
    {
        StartValue = from;
        TargetValue = to;
        TweenDuration = time;
        IsTimeScaleIgnored = ignoreTimeScale;
        onFloatTween = new OnFloatTween();
        onFloatTween.AddListener(onTween);
    }

    public void TweenValue (float tweenPercent)
    {
        if (!IsTargetValid) return;

        var newValue = Mathf.SmoothStep(StartValue, TargetValue, tweenPercent);
        onFloatTween.Invoke(newValue);
    }
}

