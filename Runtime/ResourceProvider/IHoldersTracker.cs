using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Implementation is able to track holders of Unity objects.
    /// </summary>
    public interface IHoldersTracker
    {
        /// <summary>
        /// Adds provided object as a holder of the Unity object.
        /// The Unity object won't be unloaded (destroyed), as long as it has at least one holder.
        /// </summary>
        /// <param name="obj">The Unity object to hold.</param>
        /// <param name="holder">Holder of the Unity object.</param>
        /// <returns>Total number of unique holders of the Unity object after the addition.</returns>
        int Hold (Object obj, object holder);
        /// <summary>
        /// Removes provided object from holders of the Unity object.
        /// </summary>
        /// <param name="obj">The Unity object which should no longer be held by the object.</param>
        /// <param name="holder">The holder object to remove.</param>
        /// <returns>Total number of unique holders of the Unity object after the removal.</returns>
        int Release (Object obj, object holder);
        /// <summary>
        /// Returns total number of unique holders of the Unity object.
        /// </summary>
        int CountHolders (Object obj);
    }
}
