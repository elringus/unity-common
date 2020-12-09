using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public static class SpriteUtils
    {
        public static void SetOpacity (this SpriteRenderer spriteRenderer, float opacity)
        {
            Debug.Assert(spriteRenderer != null);
            var spriteColor = spriteRenderer.color;
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, opacity);
        }

        public static bool IsTransparent (this SpriteRenderer spriteRenderer)
        {
            Debug.Assert(spriteRenderer != null);
            return Mathf.Approximately(spriteRenderer.color.a, 0f);
        }

        public static bool IsOpaque (this SpriteRenderer spriteRenderer)
        {
            Debug.Assert(spriteRenderer != null);
            return Mathf.Approximately(spriteRenderer.color.a, 1f);
        }

        /// <summary>
        /// Returns a rect, that bounds the vertices of the sprite geometry.
        /// </summary>
        public static Rect GetVerticesRect (this Sprite sprite)
        {
            var minVertPos = new Vector2(sprite.vertices.Min(v => v.x), sprite.vertices.Min(v => v.y));
            var maxVertPos = new Vector2(sprite.vertices.Max(v => v.x), sprite.vertices.Max(v => v.y));
            var spriteSizeX = Mathf.Abs(maxVertPos.x - minVertPos.x);
            var spriteSizeY = Mathf.Abs(maxVertPos.y - minVertPos.y);
            var spriteSize = new Vector2(spriteSizeX, spriteSizeY);
            return new Rect(minVertPos, spriteSize);
        }
    }
}
