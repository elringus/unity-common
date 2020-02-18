using System;

namespace UnityCommon
{
    public readonly struct FloatTween : ITweenValue
    {
        public float TweenDuration { get; }
        public EasingType EasingType { get; }
        public bool TimeScaleIgnored { get; }
        public bool TargetValid => onTween != null && (!targetProvided || target);

        private readonly float startValue;
        private readonly float targetValue;
        private readonly Action<float> onTween;
        private readonly EasingFunction easingFunction;
        private readonly UnityEngine.Object target;
        private readonly bool targetProvided;

        public FloatTween (float from, float to, float time, Action<float> onTween, 
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

            var newValue = easingFunction(startValue, targetValue, tweenPercent);
            onTween.Invoke(newValue);
        }
    }
}
