using System;
using UnityEngine;

/// <summary>
/// Often used with: MinMaxRange Attribute
/// </summary>
[Serializable]
public struct MinMaxRange
{
    [SerializeField]
    public float MinValue;

    [SerializeField]
    public float MaxValue;

    public readonly float Random()
    {
        return UnityEngine.Random.Range(MinValue, MaxValue);
    }

    public readonly bool IsInside(float value)
    {
        return value >= MinValue && value <= MaxValue;
    }

    public readonly bool IsZeroed()
    {
        return MinValue == 0 && MaxValue == 0;
    }
}

/// <summary>
/// Often used with: MinMaxRange Attribute
/// </summary>
[Serializable]
public struct MinMaxRangeInt
{
    [SerializeField]
    public int MinValue;

    [SerializeField]
    public int MaxValue;

    public readonly int Random()
    {
        return UnityEngine.Random.Range(MinValue, MaxValue + 1);
    }

    public readonly bool IsInside(int value)
    {
        return value >= MinValue && value <= MaxValue;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class MinMaxRangeAttribute : Attribute
{
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }

    public MinMaxRangeAttribute(float minValue, float maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}
