using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to track holders of Unity objects.
    /// </summary>
    public interface IHoldersTracker
    {
        int Hold (Object obj, object holder);
        int Release (Object obj, object holder);
        int CountHolders (Object obj);
    }
}
