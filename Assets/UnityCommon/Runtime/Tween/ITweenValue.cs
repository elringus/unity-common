
namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to represent a value used by <see cref="Tweener{TTweenValue}"/>.
    /// </summary>
    public interface ITweenValue
    {
        /// <summary>
        /// Whether Unity's time scale should be ignored when tweening the value.
        /// </summary>
        bool TimeScaleIgnored { get; }
        /// <summary>
        /// Whether tweened target is valid.
        /// </summary>
        bool TargetValid { get; }
        /// <summary>
        /// Duration of the tween, in seconds.
        /// </summary>
        float TweenDuration { get; }
        /// <summary>
        /// Type of animation easing function the tweener uses.
        /// </summary>
        EasingType EasingType { get; }

        /// <summary>
        /// Perform the value tween over specified ratio, in 0.0 to 1.0 range.
        /// </summary>
        void TweenValue (float tweenRatio);
    }
}
