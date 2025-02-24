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

    public void AddOrUpdate(object key, float value, ModifierType type = ModifierType.Multiplicative)
    {
        if (type == ModifierType.Additive)
            additiveModifiers[key] = value;
        else
            multiplicativeModifiers[key] = value;

        RecalculateMultiplier();
    }

    /// <summary>
    /// Remove all modifiers with this key
    /// </summary>
    /// <param name="key"></param>
    public void Remove(object key)
    {
        RemoveAdditive(key);
        RemoveMultiplicative(key);
    }

    public void RemoveMultiplicative(object key)
    {
        if (multiplicativeModifiers.Remove(key))
            RecalculateMultiplier();
    }

    public void RemoveAdditive(object key)
    {
        if (additiveModifiers.Remove(key))
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

    public enum ModifierType
    {
        Additive,
        Multiplicative,
    }
}
