using UnityEngine;
using UnityEngine.Events;

public enum ColorTweenMode { All, RGB, Alpha }

public struct ColorTween : ITweenValue
{
    class OnColorTween : UnityEvent<Color> { }

    public Color StartColor { get; set; }
    public Color TargetColor { get; set; }
    public ColorTweenMode TweenMode { get; set; }
    public float TweenDuration { get; set; }
    public bool IsTimeScaleIgnored { get; set; }
    public bool IsTargetValid { get { return onColorTween != null; } }

    private OnColorTween onColorTween;

    public ColorTween (Color from, Color to, ColorTweenMode mode, float time, UnityAction<Color> onTween, bool ignoreTimeScale = false)
    {
        StartColor = from;
        TargetColor = to;
        TweenMode = mode;
        TweenDuration = time;
        IsTimeScaleIgnored = ignoreTimeScale;
        onColorTween = new OnColorTween();
        onColorTween.AddListener(onTween);
    }

    public void TweenValue (float tweenPercent)
    {
        if (!IsTargetValid) return;

        var newColor = Color.Lerp(StartColor, TargetColor, tweenPercent);

        if (TweenMode == ColorTweenMode.Alpha)
        {
            newColor.r = StartColor.r;
            newColor.g = StartColor.g;
            newColor.b = StartColor.b;
        }
        else if (TweenMode == ColorTweenMode.RGB)
        {
            newColor.a = StartColor.a;
        }

        onColorTween.Invoke(newColor);
    }
}

