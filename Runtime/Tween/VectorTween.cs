using System;
using UnityEngine;

namespace UnityCommon
{
    public readonly struct VectorTween : ITweenValue
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
    }
}
