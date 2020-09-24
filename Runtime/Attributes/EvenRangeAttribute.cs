using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// The field will be edited as a range of even integers.
    /// </summary>
    public class EvenRangeAttribute : PropertyAttribute
    {
        public int Min { get; }
        public int Max { get; }

        public EvenRangeAttribute (int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
