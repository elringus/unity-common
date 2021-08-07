using System;
using UnityEngine;

namespace UnityCommon
{
    public readonly struct VectorTween : ITweenValue, IEquatable<VectorTween>
    {
        public float TweenDuration { get; }
        public EasingType EasingType { get; }
        public bool TimeScaleIgnored { get; }

        private readonly Vector3 startValue;
        private readonly Vector3 targetValue;
        private readonly Action<Vector3> onTween;

        public VectorTween (Vector3 from, Vector3 to, float time, Action<Vector3> onTween,
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
            var newValue = new Vector3(
                EasingType.Tween(startValue.x, targetValue.x, tweenPercent),
                EasingType.Tween(startValue.y, targetValue.y, tweenPercent),
                EasingType.Tween(startValue.z, targetValue.z, tweenPercent)
            );

            onTween.Invoke(newValue);
        }

        public bool Equals (VectorTween other)
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
            return obj is VectorTween other && Equals(other);
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

        public static bool operator == (VectorTween left, VectorTween right)
        {
            return left.Equals(right);
        }

        public static bool operator != (VectorTween left, VectorTween right)
        {
            return !left.Equals(right);
        }
    }
}
