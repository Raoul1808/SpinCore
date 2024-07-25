using System.Collections.Generic;
using UnityEngine;

namespace SpinCore
{
    /// <summary>
    /// A class that contains a bunch of extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Clamps a number within the <see cref="IntRange"/>
        /// </summary>
        /// <param name="range">The range</param>
        /// <param name="index">The number to clamp</param>
        /// <returns>The clamped number</returns>
        public static int ClampInt(this IntRange range, int index)
        {
            if (index >= range.max)
                index = range.max - 1;
            if (index < 0)
                index = 0;
            return index;
        }

        /// <summary>
        /// Removes all children from a given transform.
        /// </summary>
        /// <param name="transform">The transform to remove children from</param>
        public static void RemoveAllChildren(this Transform transform) => transform.RemoveAllChildrenUntilIndex(0);

        /// <summary>
        /// Removes all children in reverse order until the given index.
        /// </summary>
        /// <param name="transform">The transform to remove children from</param>
        /// <param name="index">The index to stop at</param>
        public static void RemoveAllChildrenUntilIndex(this Transform transform, int index)
        {
            for (int i = transform.childCount; i > index; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i - 1).gameObject);
            }
        }

        /// <summary>
        /// Gets and removes the value associated with the specified key.
        /// Functions identically to <c>dict.TryGetValue(key, out value)</c> followed by <c>dict.Remove(key)</c>.
        /// </summary>
        /// <param name="dict">The dictionary</param>
        /// <param name="key">The key of the value to get</param>
        /// <param name="value">The value if the key is found, or a default value otherwise</param>
        /// <returns>true if the value was found and removed; false otherwise</returns>
        public static bool TryPop<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (!dict.TryGetValue(key, out value)) return false;
            dict.Remove(key);
            return true;
        }
    }
}
