using System.Collections.Generic;
using UnityEngine;

namespace SpinCore
{
    public static class Extensions
    {
        public static int ClampInt(this IntRange range, int index)
        {
            if (index >= range.max)
                index = range.max - 1;
            if (index < 0)
                index = 0;
            return index;
        }

        public static void RemoveAllChildren(this Transform transform) => transform.RemoveAllChildrenUntilIndex(0);

        public static void RemoveAllChildrenUntilIndex(this Transform transform, int index)
        {
            for (int i = transform.childCount; i > index; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i - 1).gameObject);
            }
        }

        public static bool TryPop<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (!dict.TryGetValue(key, out value)) return false;
            dict.Remove(key);
            return true;
        }
    }
}
