using System;
using System.Collections.Generic;

public static class LinqUtils
{
    /// <summary>
    /// Removes last item in the list.
    /// </summary>
    public static void RemoveLastItem<T> (this List<T> list, Predicate<T> predicate = null)
    {
        if (list == null || list.Count == 0) return;

        var elementIndex = predicate == null ? list.Count - 1 : list.FindLastIndex(predicate);
        if (elementIndex >= 0)
            list.RemoveAt(elementIndex);
    }
}
