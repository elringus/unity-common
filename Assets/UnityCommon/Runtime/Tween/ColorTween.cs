using System;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents available tween modes for <see cref="Color"/> values.
    /// </summary>
    public enum ColorTweenMode { All, RGB, Alpha }

    public readonly struct ColorTween : ITweenValue
    {
        public EasingType EasingType { get; }
        public float TweenDuration { get; }
        public bool TimeScaleIgnored { get; }
        public bool TargetValid => onTween != null && (!targetProvided || target);

        private readonly Color startColor;
        private readonly Color targetColor;
        private readonly ColorTweenMode tweenMode;
        private readonly Action<Color> onTween;
        private readonly EasingFunction easingFunction;
        private readonly UnityEngine.Object target;
        private readonly bool targetProvided;

        public ColorTween (Color from, Color to, ColorTweenMode mode, float time, Action<Color> onTween, 
            bool ignoreTimeScale = false, EasingType easingType = default, UnityEngine.Object target = default)
        {
            startColor = from;
            targetColor = to;
            tweenMode = mode;
            TweenDuration = time;
            EasingType = easingType;
            TimeScaleIgnored = ignoreTimeScale;
            this.onTween = onTween;

            targetProvided = this.target = target;
            easingFunction = EasingType.GetEasingFunction();
        }

        public void TweenValue (float tweenPercent)
        {
            if (!TargetValid) return;

            var newColor = default(Color);
            newColor.r = tweenMode == ColorTweenMode.Alpha ? startColor.r : easingFunction(startColor.r, targetColor.r, tweenPercent);
            newColor.g = tweenMode == ColorTweenMode.Alpha ? startColor.g : easingFunction(startColor.g, targetColor.g, tweenPercent);
            newColor.b = tweenMode == ColorTweenMode.Alpha ? startColor.b : easingFunction(startColor.b, targetColor.b, tweenPercent);
            newColor.a = tweenMode == ColorTweenMode.RGB ? startColor.a : easingFunction(startColor.a, targetColor.a, tweenPercent);

            onTween.Invoke(newColor);
        }
    }
}
