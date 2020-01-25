using System;
using UnityEngine;

namespace UnityCommon
{
    public struct VectorTween : ITweenValue
    {
        public event Action<Vector3> OnTween;

        public Vector3 StartValue { get; set; }
        public Vector3 TargetValue { get; set; }
        public float TweenDuration { get; set; }
        public EasingType EasingType { get; }
        public bool TimeScaleIgnored { get; set; }
        public bool TargetValid => OnTween != null;

        private readonly EasingFunction easingFunction;

        public VectorTween (Vector3 from, Vector3 to, float time, Action<Vector3> onTween, bool ignoreTimeScale = false, EasingType easingType = default)
        {
            StartValue = from;
            TargetValue = to;
            TweenDuration = time;
            EasingType = easingType;
            TimeScaleIgnored = ignoreTimeScale;
            OnTween = onTween;

            easingFunction = EasingType.GetEasingFunction();
        }

        public void TweenValue (float tweenPercent)
        {
            if (!TargetValid) return;

            var newValue = new Vector3(
                easingFunction(StartValue.x, TargetValue.x, tweenPercent),
                easingFunction(StartValue.y, TargetValue.y, tweenPercent),
                easingFunction(StartValue.z, TargetValue.z, tweenPercent)
            );

            OnTween.Invoke(newValue);
        }
    }
}
