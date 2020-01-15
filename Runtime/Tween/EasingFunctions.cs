using UnityEngine;

namespace UnityCommon
{
    public delegate float EasingFunction (float start, float end, float value);

    public static class EasingFunctions
    {
        /// <summary>
        /// Returns an <see cref="EasingFunction"/> corresponding to the type of easing.
        /// Be aware that this method allocates memory; consider caching the returned delegate.
        /// </summary>
        public static EasingFunction GetEasingFunction (this EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.Linear: return Linear;
                case EasingType.SmoothStep: return SmoothStep;
                case EasingType.Spring: return Spring;
                case EasingType.EaseInQuad: return EaseInQuad;
                case EasingType.EaseOutQuad: return EaseOutQuad;
                case EasingType.EaseInOutQuad: return EaseInOutQuad;
                case EasingType.EaseInCubic: return EaseInCubic;
                case EasingType.EaseOutCubic: return EaseOutCubic;
                case EasingType.EaseInOutCubic: return EaseInOutCubic;
                case EasingType.EaseInQuart: return EaseInQuart;
                case EasingType.EaseOutQuart: return EaseOutQuart;
                case EasingType.EaseInOutQuart: return EaseInOutQuart;
                case EasingType.EaseInQuint: return EaseInQuint;
                case EasingType.EaseOutQuint: return EaseOutQuint;
                case EasingType.EaseInOutQuint: return EaseInOutQuint;
                case EasingType.EaseInSine: return EaseInSine;
                case EasingType.EaseOutSine: return EaseOutSine;
                case EasingType.EaseInOutSine: return EaseInOutSine;
                case EasingType.EaseInExpo: return EaseInExpo;
                case EasingType.EaseOutExpo: return EaseOutExpo;
                case EasingType.EaseInOutExpo: return EaseInOutExpo;
                case EasingType.EaseInCirc: return EaseInCirc;
                case EasingType.EaseOutCirc: return EaseOutCirc;
                case EasingType.EaseInOutCirc: return EaseInOutCirc;
                case EasingType.EaseInBounce: return EaseInBounce;
                case EasingType.EaseOutBounce: return EaseOutBounce;
                case EasingType.EaseInOutBounce: return EaseInOutBounce;
                case EasingType.EaseInBack: return EaseInBack;
                case EasingType.EaseOutBack: return EaseOutBack;
                case EasingType.EaseInOutBack: return EaseInOutBack;
                case EasingType.EaseInElastic: return EaseInElastic;
                case EasingType.EaseOutElastic: return EaseOutElastic;
                case EasingType.EaseInOutElastic: return EaseInOutElastic;
                default: return null;
            }
        }

        public static float Linear (float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value);
        }

        public static float SmoothStep (float start, float end, float value)
        {
            return Mathf.SmoothStep(start, end, value);
        }

        public static float Spring (float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        public static float EaseInQuad (float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        public static float EaseOutQuad (float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        public static float EaseInOutQuad (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * .5f * value * value + start;
            value--;
            return -end * .5f * (value * (value - 2) - 1) + start;
        }

        public static float EaseInCubic (float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        public static float EaseOutCubic (float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        public static float EaseInOutCubic (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * .5f * value * value * value + start;
            value -= 2;
            return end * .5f * (value * value * value + 2) + start;
        }

        public static float EaseInQuart (float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }

        public static float EaseOutQuart (float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        public static float EaseInOutQuart (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * .5f * value * value * value * value + start;
            value -= 2;
            return -end * .5f * (value * value * value * value - 2) + start;
        }

        public static float EaseInQuint (float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        public static float EaseOutQuint (float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        public static float EaseInOutQuint (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * .5f * value * value * value * value * value + start;
            value -= 2;
            return end * .5f * (value * value * value * value * value + 2) + start;
        }

        public static float EaseInSine (float start, float end, float value)
        {
            end -= start;
            return -end * Mathf.Cos(value * (Mathf.PI * .5f)) + end + start;
        }

        public static float EaseOutSine (float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Sin(value * (Mathf.PI * .5f)) + start;
        }

        public static float EaseInOutSine (float start, float end, float value)
        {
            end -= start;
            return -end * .5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
        }

        public static float EaseInExpo (float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (value - 1)) + start;
        }

        public static float EaseOutExpo (float start, float end, float value)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
        }

        public static float EaseInOutExpo (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * .5f * Mathf.Pow(2, 10 * (value - 1)) + start;
            value--;
            return end * .5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
        }

        public static float EaseInCirc (float start, float end, float value)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        public static float EaseOutCirc (float start, float end, float value)
        {
            value--;
            end -= start;
            return end * Mathf.Sqrt(1 - value * value) + start;
        }

        public static float EaseInOutCirc (float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return -end * .5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
            value -= 2;
            return end * .5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
        }

        public static float EaseInBounce (float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            return end - EaseOutBounce(0, end, d - value) + start;
        }

        public static float EaseOutBounce (float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }
            else if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }
            else if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }
            else
            {
                value -= (2.625f / 2.75f);
                return end * (7.5625f * (value) * value + .984375f) + start;
            }
        }

        public static float EaseInOutBounce (float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d * .5f) return EaseInBounce(0, end, value * 2) * .5f + start;
            else return EaseOutBounce(0, end, value * 2 - d) * .5f + end * .5f + start;
        }

        public static float EaseInBack (float start, float end, float value)
        {
            end -= start;
            value /= 1;
            float s = 1.70158f;
            return end * value * value * ((s + 1) * value - s) + start;
        }

        public static float EaseOutBack (float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = value - 1;
            return end * (value * value * ((s + 1) * value + s) + 1) + start;
        }

        public static float EaseInOutBack (float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1)
            {
                s *= (1.525f);
                return end * .5f * (value * value * ((s + 1) * value - s)) + start;
            }
            value -= 2;
            s *= (1.525f);
            return end * .5f * ((value) * value * ((s + 1) * value + s) + 2) + start;
        }

        public static float EaseInElastic (float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        public static float EaseOutElastic (float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * .25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start;
        }

        public static float EaseInOutElastic (float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d * .5f) == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1) return -.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * .5f + end + start;
        }
    }
}