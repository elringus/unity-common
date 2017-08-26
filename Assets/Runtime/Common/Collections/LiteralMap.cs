using System;
using System.Collections.Generic;

/// <summary>
/// Dictionary with case-insensitive string keys.
/// </summary>
public class LiteralMap<TValue> : Dictionary<string, TValue>
{
    public LiteralMap () : base(StringComparer.OrdinalIgnoreCase) { }
}
