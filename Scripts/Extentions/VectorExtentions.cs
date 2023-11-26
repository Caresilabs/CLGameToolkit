using UnityEngine;

public static class VectorExtentions
{
    public static Vector3 XZ(this Vector3 vector, float overrideY = 0f)
    {
        return new Vector3(vector.x, overrideY, vector.z);
    }

    public static Vector3 Parse(string vector)
    {
        if (vector == null || vector == "")
        {
            return Vector3.zero;
        }

        var stripped = vector
            .Replace("(", "")
               .Replace(")", "")
                  .Replace(" ", "");

        var array = stripped.Split(',');
        return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
    }


    public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
    {
        Vector3 A = target.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.localScale.x; // relataive scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        target.localScale = newScale;
        target.localPosition = FP;
    }

}
