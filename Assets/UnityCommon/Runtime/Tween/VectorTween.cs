using System;
using UnityEngine;

namespace UnityCommon
{
    public readonly struct VectorTween : ITweenValue, IEquatable<VectorTween>
    {
        public float TweenDuration { get; }
        public EasingType EasingType { get; }
        public bool TimeScaleIgnored { get; }
        public bool TargetValid => onTween != null && (!targetProvided || target);

        private readonly Vector3 startValue;
        private readonly Vector3 targetValue;
        private readonly Action<Vector3> onTween;
        private readonly EasingFunction easingFunction;
        private readonly UnityEngine.Object target;
        private readonly bool targetProvided;

        public VectorTween (Vector3 from, Vector3 to, float time, Action<Vector3> onTween, 
            bool ignoreTimeScale = false, EasingType easingType = default, UnityEngine.Object target = default)
        {
            startValue = from;
            targetValue = to;
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

            var newValue = new Vector3(
                easingFunction(startValue.x, targetValue.x, tweenPercent),
                easingFunction(startValue.y, targetValue.y, tweenPercent),
                easingFunction(startValue.z, targetValue.z, tweenPercent)
            );

            onTween.Invoke(newValue);
        }
        
        public bool Equals (VectorTween other)
        {
            return startValue.Equals(other.startValue) && 
                   targetValue.Equals(other.targetValue) && 
                   Equals(onTween, other.onTween) && 
                   Equals(easingFunction, other.easingFunction) && 
                   Equals(target, other.target) && 
                   targetProvided == other.targetProvided && 
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
                hashCode = (hashCode * 397) ^ (easingFunction != null ? easingFunction.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (target != null ? target.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ targetProvided.GetHashCode();
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
