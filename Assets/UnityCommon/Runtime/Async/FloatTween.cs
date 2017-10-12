using UnityEngine;
using UnityEngine.Events;

public struct FloatTween : ITweenValue
{
    public event UnityAction<float> OnFloatTween;

    public float StartValue { get; set; }
    public float TargetValue { get; set; }
    public float TweenDuration { get; set; }
    public bool IsTimeScaleIgnored { get; set; }
    public bool IsTargetValid { get { return OnFloatTween != null; } }

    public FloatTween (float from, float to, float time, UnityAction<float> onTween, bool ignoreTimeScale = false)
    {
        StartValue = from;
        TargetValue = to;
        TweenDuration = time;
        IsTimeScaleIgnored = ignoreTimeScale;
        OnFloatTween = onTween;
    }

    public void TweenValue (float tweenPercent)
    {
        if (!IsTargetValid) return;

        var newValue = Mathf.Lerp(StartValue, TargetValue, tweenPercent);
        OnFloatTween.Invoke(newValue);
    }
}
