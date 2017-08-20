using System;
using System.Collections.Generic;

/// <summary>
/// Dictionary with case-insensitive string keys.
/// </summary>
public class LiteralMap<TValue> : SerializableDictionary<string, TValue>
{
    public override IEqualityComparer<string> Comparer { get { return StringComparer.OrdinalIgnoreCase; } }
}

/// <summary>
/// A serializable version of LiteralMap with string values.
/// </summary>
[Serializable]
public class LiteralStringMap : LiteralMap<string> { }

