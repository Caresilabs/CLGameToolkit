using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("CLGameToolkit/Fog")]
public class FogVolumeComponent : VolumeComponent
{
    public FogModeParameter FogMode = new FogModeParameter(UnityEngine.FogMode.Exponential);
    public ColorParameter FogColor = new ColorParameter(Color.black);

    [Header("Linear")]
    public FloatParameter FogDistanceStart = new MinFloatParameter(0, 0);
    public FloatParameter FogDistanceEnd = new MinFloatParameter(0, 0);

    [Header("Exponential")]
    public FloatParameter FogDensity = new MinFloatParameter(0, 0);

    protected override void OnEnable()
    {
        // Hack: Check if this is instanced in scene view or if it is created by ScriptableObject.CreateInstace
        //if (hideFlags == HideFlags.None)
        //{
        //    // Set default values from RenderSettings
        //    FogMode.value = RenderSettings.fogMode;
        //    FogColor.value = RenderSettings.fogColor;

        //    FogDistanceStart.value = RenderSettings.fogStartDistance;
        //    FogDistanceEnd.value = RenderSettings.fogEndDistance;

        //    FogDensity.value = RenderSettings.fogDensity;
        //}

        base.OnEnable();
    }


    public override void Override(VolumeComponent state, float interpFactor)
    {
        base.Override(state, interpFactor);

        if (!Application.isPlaying) return;

        FogVolumeComponent fogState = state as FogVolumeComponent;

        if (FogColor.overrideState) RenderSettings.fogColor = fogState.FogColor.value;
        if (FogMode.overrideState) RenderSettings.fogMode = fogState.FogMode.value;

        if (RenderSettings.fogMode == UnityEngine.FogMode.Linear)
        {
            if (FogDistanceStart.overrideState) RenderSettings.fogStartDistance = fogState.FogDistanceStart.value;
            if (FogDistanceEnd.overrideState) RenderSettings.fogEndDistance = fogState.FogDistanceEnd.value;
        }
        else
        {
            if (FogDensity.overrideState) RenderSettings.fogDensity = FogDensity.value;
        }
    }

}

[Serializable]
public sealed class FogModeParameter : VolumeParameter<FogMode>
{
    public FogModeParameter(FogMode value, bool overrideState = false) : base(value, overrideState) { }
}
