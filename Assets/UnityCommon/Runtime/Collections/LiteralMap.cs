using System;

namespace UnityCommon
{
    /// <summary>
    /// Dictionary with case-insensitive string keys.
    /// </summary>
    public class LiteralMap<TValue> : Map<string, TValue>
    {
        public LiteralMap () : base(StringComparer.OrdinalIgnoreCase) { }
    }

    /// <summary>
    /// A serializable version of <see cref="LiteralMap{TValue}"/> with string values.
    /// </summary>
    [Serializable]
    public class SerializableLiteralStringMap : LiteralMap<string> { }
}
