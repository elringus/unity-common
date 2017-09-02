using UnityEngine;
using UnityEngine.Events;

public static class TransformUtils
{
    public static void AddPosX (this Transform trs, float x)
    {
        trs.position += new Vector3(x, 0, 0);
    }

    public static void AddPosY (this Transform trs, float y)
    {
        trs.position += new Vector3(0, y, 0);
    }

    public static void AddPosZ (this Transform trs, float z)
    {
        trs.position += new Vector3(0, 0, z);
    }

    public static void SetPosX (this Transform trs, float x, bool local = false)
    {
        if (local) trs.localPosition = new Vector3(x, trs.localPosition.y, trs.localPosition.z);
        else trs.position = new Vector3(x, trs.position.y, trs.position.z);
    }

    public static void SetPosY (this Transform trs, float y, bool local = false)
    {
        if (local) trs.localPosition = new Vector3(trs.localPosition.x, y, trs.localPosition.z);
        else trs.position = new Vector3(trs.position.x, y, trs.position.z);
    }

    public static void SetPosZ (this Transform trs, float z, bool local = false)
    {
        if (local) trs.localPosition = new Vector3(trs.localPosition.x, trs.localPosition.y, z);
        else trs.position = new Vector3(trs.position.x, trs.position.y, z);
    }

    public static Tweener<FloatTween> SlideX (this Transform trs, float x, float duration,
        MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        if (trs.position.x == x) { onComplete.SafeInvoke(); return null; }
        if (duration == 0f) { trs.SetPosX(x); onComplete.SafeInvoke(); return null; }

        var tween = new FloatTween(trs.position.x, x, duration, newXValue => trs.SetPosX(newXValue));
        return new Tweener<FloatTween>(coroutineContainer, onComplete).Run(tween);
    }
}
