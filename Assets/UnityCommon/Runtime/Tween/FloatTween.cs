using System;

namespace UnityCommon
{
    public readonly struct FloatTween : ITweenValue, IEquatable<FloatTween>
    {
        public float TweenDuration { get; }
        public EasingType EasingType { get; }
        public bool TimeScaleIgnored { get; }

        private readonly float startValue;
        private readonly float targetValue;
        private readonly Action<float> onTween;

        public FloatTween (float from, float to, float time, Action<float> onTween,
            bool ignoreTimeScale = false, EasingType easingType = default)
        {
            startValue = from;
            targetValue = to;
            TweenDuration = time;
            EasingType = easingType;
            TimeScaleIgnored = ignoreTimeScale;
            this.onTween = onTween;
        }

        public void TweenValue (float tweenPercent)
        {
            var newValue = EasingType.Tween(startValue, targetValue, tweenPercent);
            onTween.Invoke(newValue);
        }

        public bool Equals (FloatTween other)
        {
            return startValue.Equals(other.startValue) &&
                   targetValue.Equals(other.targetValue) &&
                   Equals(onTween, other.onTween) &&
                   TweenDuration.Equals(other.TweenDuration) &&
                   EasingType == other.EasingType &&
                   TimeScaleIgnored == other.TimeScaleIgnored;
        }

        public override bool Equals (object obj)
        {
            return obj is FloatTween other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = startValue.GetHashCode();
                hashCode = (hashCode * 397) ^ targetValue.GetHashCode();
                hashCode = (hashCode * 397) ^ (onTween != null ? onTween.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TweenDuration.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)EasingType;
                hashCode = (hashCode * 397) ^ TimeScaleIgnored.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator == (FloatTween left, FloatTween right)
        {
            return left.Equals(right);
        }

        public static bool operator != (FloatTween left, FloatTween right)
        {
            return !left.Equals(right);
        }
    }
}
