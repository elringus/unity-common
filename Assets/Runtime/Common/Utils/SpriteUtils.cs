using UnityEngine;
using UnityEngine.Events;

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

    public static Tweener<FloatTween> FadeOut (this SpriteRenderer spriteRenderer, float fadeTime = .25f,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Debug.Assert(spriteRenderer != null);
        if (spriteRenderer.IsTransparent()) { onComplete.SafeInvoke(); return null; }

        if (fadeTime == 0f) { spriteRenderer.SetOpacity(0f); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(spriteRenderer.color.a, 0f, fadeTime, a => spriteRenderer.SetOpacity(a));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }

    public static Tweener<FloatTween> FadeIn (this SpriteRenderer spriteRenderer, float fadeTime = .25f,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Debug.Assert(spriteRenderer != null);
        if (spriteRenderer.IsOpaque()) { onComplete.SafeInvoke(); return null; }

        if (fadeTime == 0f) { spriteRenderer.SetOpacity(1f); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(spriteRenderer.color.a, 1f, fadeTime, a => spriteRenderer.SetOpacity(a));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }
}

