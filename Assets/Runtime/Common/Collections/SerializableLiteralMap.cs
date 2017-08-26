using System;
using System.Collections.Generic;

public abstract class SerializableLiteralMap<TValue> : SerializableDictionary<string, TValue>
{
    public override IEqualityComparer<string> Comparer { get { return StringComparer.OrdinalIgnoreCase; } }
}

/// <summary>
/// A serializable version of LiteralMap with string values.
/// </summary>
[Serializable]
public class SerializableLiteralStringMap : SerializableLiteralMap<string> { }
