using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class GraphicUtils
{
    public static void SetOpacity (this Graphic graphic, float opacity)
    {
        Debug.Assert(graphic != null);
        var spriteColor = graphic.color;
        graphic.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, opacity);
    }

    public static bool IsTransparent (this Graphic graphic)
    {
        Debug.Assert(graphic != null);
        return Mathf.Approximately(graphic.color.a, 0f);
    }

    public static bool IsOpaque (this Graphic graphic)
    {
        Debug.Assert(graphic != null);
        return Mathf.Approximately(graphic.color.a, 1f);
    }

    public static Tweener<FloatTween> FadeOut (this Graphic graphic, float fadeTime = .25f,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Debug.Assert(graphic != null);
        if (graphic.IsTransparent()) { onComplete.SafeInvoke(); return null; }

        if (fadeTime == 0f) { graphic.SetOpacity(0f); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(graphic.color.a, 0f, fadeTime, a => graphic.SetOpacity(a));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }

    public static Tweener<FloatTween> FadeIn (this Graphic graphic, float fadeTime = .25f,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Debug.Assert(graphic != null);
        if (graphic.IsOpaque()) { onComplete.SafeInvoke(); return null; }

        if (fadeTime == 0f) { graphic.SetOpacity(1f); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(graphic.color.a, 1f, fadeTime, a => graphic.SetOpacity(a));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }

    public static Tweener<FloatTween> Fade (this Graphic graphic, float targetOpacity, float fadeTime = .25f,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Debug.Assert(graphic != null);
        if (graphic.color.a == targetOpacity) { onComplete.SafeInvoke(); return null; }

        if (fadeTime == 0f) { graphic.SetOpacity(targetOpacity); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(graphic.color.a, targetOpacity, fadeTime, a => graphic.SetOpacity(a));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }
}
