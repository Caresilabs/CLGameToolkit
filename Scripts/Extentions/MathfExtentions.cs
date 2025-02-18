using UnityEngine;

public static class MathfExtentions
{
    public static float RoundToNearestMultiple(float value, float multiple)
    {
        return Mathf.Round(value / multiple) * multiple;
    }

    public static int RoundToNearestMultiple(int value, int multiple)
    {
        return Mathf.RoundToInt(Mathf.Round(value / (float)multiple) * multiple);
    }

    public static Vector3 RoundToNearestMultiple(Vector3 value, int multiple)
    {
        value.x = RoundToNearestMultiple(value.x, multiple);
        value.y = RoundToNearestMultiple(value.y, multiple);
        value.z = RoundToNearestMultiple(value.z, multiple);
        return value;
    }

    public static int CyclePostitive(int value, int maxValue)
    {
        return (value % maxValue + maxValue) % maxValue;
    }

    public static float ClampSign(float value)
    {
        if (value < -1)
            value = -1;
        else if (value > 1)
            value = 1;

        return value;
    }

    public static bool ChancePerSecondFast(float probabilityPerSecond)
    {
        return Random.value < probabilityPerSecond * Time.deltaTime;
    }

    public static bool ChancePerSecond(float probabilityPerSecond)
    {
        float probabilityThisFrame = 1 - Mathf.Exp(-probabilityPerSecond * Time.deltaTime);

        return Random.value < probabilityThisFrame;
    }

    public static float ClampAngle180(float angle)
    {
        angle %= 360;
        return angle > 180 ? angle - 360 : angle < -180 ? angle + 360 : angle;
    }
}
