using System;

namespace UnityCommon
{
    public struct FloatTween : ITweenValue
    {
        public event Action<float> OnFloatTween;

        public float StartValue { get; set; }
        public float TargetValue { get; set; }
        public float TweenDuration { get; set; }
        public EasingType EasingType { get; }
        public bool IsTimeScaleIgnored { get; set; }
        public bool IsTargetValid => OnFloatTween != null;

        private readonly EasingFunction easingFunction;

        public FloatTween (float from, float to, float time, Action<float> onTween, bool ignoreTimeScale = false, EasingType easingType = default)
        {
            StartValue = from;
            TargetValue = to;
            TweenDuration = time;
            EasingType = easingType;
            IsTimeScaleIgnored = ignoreTimeScale;
            OnFloatTween = onTween;

            easingFunction = EasingType.GetEasingFunction();
        }

        public void TweenValue (float tweenPercent)
        {
            if (!IsTargetValid) return;

            var newValue = easingFunction(StartValue, TargetValue, tweenPercent);
            OnFloatTween.Invoke(newValue);
        }

    }
}
