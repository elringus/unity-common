using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityCommon
{
    /// <summary>
    /// Dictionary with case-insensitive string keys.
    /// </summary>
    public class LiteralMap<TValue> : Dictionary<string, TValue>
    {
        public LiteralMap () : base(StringComparer.OrdinalIgnoreCase) { }
        public LiteralMap (IDictionary<string, TValue> dictionary) : base(dictionary, StringComparer.OrdinalIgnoreCase) { }
    }

    /// <summary>
    /// A serializable version of <see cref="LiteralMap{TValue}"/> with <see cref="string"/> values.
    /// </summary>
    [Serializable]
    public class SerializableLiteralStringMap : SerializableMap<string, string>
    {
        public SerializableLiteralStringMap () : base(StringComparer.OrdinalIgnoreCase) { }
        public SerializableLiteralStringMap (IDictionary<string, string> dictionary) : base(dictionary, StringComparer.OrdinalIgnoreCase) { }

        public static implicit operator SerializableLiteralStringMap ((string key, string value)[] pairs)
            => new SerializableLiteralStringMap(pairs.ToDictionary(p => p.key, p => p.value));
    }
}
