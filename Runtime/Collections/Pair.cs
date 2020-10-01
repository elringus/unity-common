using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// Represents a container for two generic items.
    /// </summary>
    /// <typeparam name="T1">First item type.</typeparam>
    /// <typeparam name="T2">Second item type.</typeparam> 
    [System.Serializable]
    public class Pair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        private static readonly IEqualityComparer<T1> item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> item2Comparer = EqualityComparer<T2>.Default;

        public Pair (T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override string ToString ()
        {
            return $"<{Item1}, {Item2}>";
        }
        
        private static bool IsNull (object obj)
        {
            return ReferenceEquals(obj, null);
        }
    }
}
   
