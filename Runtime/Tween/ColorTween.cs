using System;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents available tween modes for <see cref="Color"/> values.
    /// </summary>
    public enum ColorTweenMode { All, RGB, Alpha }

    public readonly struct ColorTween : ITweenValue, IEquatable<ColorTween>
    {
        public EasingType EasingType { get; }
        public float TweenDuration { get; }
        public bool TimeScaleIgnored { get; }
        public bool TargetValid => onTween != null && (!targetProvided || target);

        private readonly Color startColor;
        private readonly Color targetColor;
        private readonly ColorTweenMode tweenMode;
        private readonly Action<Color> onTween;
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
        }

        public void TweenValue (float tweenPercent)
        {
            if (!TargetValid) return;

            var newColor = default(Color);
            newColor.r = tweenMode == ColorTweenMode.Alpha ? startColor.r : EasingType.Tween(startColor.r, targetColor.r, tweenPercent);
            newColor.g = tweenMode == ColorTweenMode.Alpha ? startColor.g : EasingType.Tween(startColor.g, targetColor.g, tweenPercent);
            newColor.b = tweenMode == ColorTweenMode.Alpha ? startColor.b : EasingType.Tween(startColor.b, targetColor.b, tweenPercent);
            newColor.a = tweenMode == ColorTweenMode.RGB ? startColor.a : EasingType.Tween(startColor.a, targetColor.a, tweenPercent);

            onTween.Invoke(newColor);
        }
        
        public bool Equals (ColorTween other)
        {
            return startColor.Equals(other.startColor) && 
                   targetColor.Equals(other.targetColor) && 
                   tweenMode == other.tweenMode && Equals(onTween, other.onTween) && 
                   Equals(target, other.target) && 
                   targetProvided == other.targetProvided && 
                   EasingType == other.EasingType && 
                   TweenDuration.Equals(other.TweenDuration) && 
                   TimeScaleIgnored == other.TimeScaleIgnored;
        }

        public override bool Equals (object obj)
        {
            return obj is ColorTween other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = startColor.GetHashCode();
                hashCode = (hashCode * 397) ^ targetColor.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)tweenMode;
                hashCode = (hashCode * 397) ^ (onTween != null ? onTween.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (target != null ? target.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ targetProvided.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)EasingType;
                hashCode = (hashCode * 397) ^ TweenDuration.GetHashCode();
                hashCode = (hashCode * 397) ^ TimeScaleIgnored.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator == (ColorTween left, ColorTween right)
        {
            return left.Equals(right);
        }

        public static bool operator != (ColorTween left, ColorTween right)
        {
            return !left.Equals(right);
        }
    }
}
