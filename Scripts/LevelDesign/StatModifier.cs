using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatModifier
{
    // Base damage multiplier (e.g., 1.0 = no modification)
    [SerializeField] private float BaseMultiplier = 1.0f;

    private readonly Dictionary<object, float> additiveModifiers = new();
    private readonly Dictionary<object, float> multiplicativeModifiers = new();

    private float cachedFinalMultiplier = 1f; // TODO: WARN: This won't initialize correctly BaseMultiplier is serialized!!! 
    public float Value => cachedFinalMultiplier;

    public StatModifier() { }
    public StatModifier(float baseMultiplier)
    {
        this.BaseMultiplier = baseMultiplier;
        this.cachedFinalMultiplier = baseMultiplier;
    }

    public void AddOrUpdate(object key, float value, StatsModifierType type = StatsModifierType.Additive)
    {
        if (type == StatsModifierType.Additive)
            additiveModifiers[key] = value;
        else
            multiplicativeModifiers[key] = value;

        RecalculateMultiplier();
    }

    public void Remove(object key, StatsModifierType type = StatsModifierType.Additive)
    {
        Dictionary<object, float> dict = null;
        switch (type)
        {
            case StatsModifierType.Additive:
                dict = additiveModifiers;
                break;
            case StatsModifierType.Multiplicative:
                dict = multiplicativeModifiers;
                break;
            default:
                break;
        }

        if (dict != null && dict.Remove(key))
            RecalculateMultiplier();
    }

    public void RemoveMultiplicative(object key)
    {
        if (multiplicativeModifiers.Remove(key))
            RecalculateMultiplier();
    }

    private void RecalculateMultiplier()
    {
        float additiveSum = 0f;
        foreach (var mod in additiveModifiers.Values)
            additiveSum += mod;

        float multiplicativeProduct = 1f;
        foreach (var mod in multiplicativeModifiers.Values)
            multiplicativeProduct *= mod;

        cachedFinalMultiplier = (BaseMultiplier + additiveSum) * multiplicativeProduct;
        Logger.Debug("Stats Multiplier Recalculated: " + cachedFinalMultiplier);
    }

    public enum StatsModifierType
    {
        Additive,
        Multiplicative,
    }
}
