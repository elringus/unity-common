using UnityEngine;

public static class MathUtils
{
    public static bool IsEven (this int intValue)
    {
        return intValue % 2 == 0;
    }

    public static Vector3 SmoothStep (Vector3 from, Vector3 to, float progress)
    {
        var x = Mathf.SmoothStep(from.x, to.x, progress);
        var y = Mathf.SmoothStep(from.y, to.y, progress);
        var z = Mathf.SmoothStep(from.z, to.z, progress);

        return new Vector3(x, y, z);
    }

    public static bool IsWithin (this int value, int minimum, int maximum)
    {
        return value >= minimum && value <= maximum;
    }

    public static bool IsWithin (this float value, float minimum, float maximum)
    {
        return value >= minimum && value <= maximum;
    }
}
