using UnityEngine;

namespace UnityCommon
{
    public static class MaskUtils
    {
        public static void SetLayer (ref int mask, int layer, bool enabled)
        {
            if (enabled) mask = mask | (1 << layer);
            else mask = mask | ~(1 << layer);
        }

        public static bool GetLayer (int mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public static void SetLayer (ref int mask, string layerName, bool enabled)
        {
            var layer = LayerMask.NameToLayer(layerName);
            SetLayer(ref mask, layer, enabled);
        }

        public static bool GetLayer (int mask, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            return GetLayer(mask, layer);
        }
    }
}
