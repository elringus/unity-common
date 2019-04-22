
namespace UnityCommon
{
    public interface ITweenValue
    {
        bool IsTimeScaleIgnored { get; }
        bool IsTargetValid { get; }
        float TweenDuration { get; }
        EasingType EasingType { get; }

        void TweenValue (float tweenPercent);
    }
}
