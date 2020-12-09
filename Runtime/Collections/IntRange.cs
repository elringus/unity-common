using System;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents an integer range starting with <see cref="StartIndex"/> and ending with <see cref="EndIndex"/>.
    /// Both endpoints are considered to be included.
    /// </summary>
    [Serializable]
    public struct IntRange : IEquatable<IntRange>
    {
        public int StartIndex => startIndex;
        public int EndIndex => endIndex;

        [SerializeField] private int startIndex;
        [SerializeField] private int endIndex;

        public IntRange (int startIndex, int endIndex)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        public bool Contains (int index)
        {
            return index >= StartIndex && index <= EndIndex;
        }
        
        public bool Equals (IntRange other)
        {
            return startIndex == other.startIndex && endIndex == other.endIndex;
        }

        public override bool Equals (object obj)
        {
            return obj is IntRange other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                
                // ReSharper disable NonReadonlyMemberInGetHashCode
                // It's actually read-only (at runtime, that is).
                return (startIndex * 397) ^ endIndex;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public static bool operator == (IntRange left, IntRange right)
        {
            return left.Equals(right);
        }

        public static bool operator != (IntRange left, IntRange right)
        {
            return !left.Equals(right);
        }
    }
}
