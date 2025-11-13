using UnityEngine;
using UnityEngine.Rendering;

public struct EnvironmentalLightingSettings
{
    public Material SkyboxMaterial;
    public Light SunSource;
    public Color RealtimeShadowColor;
    
    public AmbientMode EnvironmentLightingSource;
    public Color EnvironmentLightingSkyColor;
    public Color EnvironmentLightingEquatorColor;
    public Color EnvironmentLightingGroundColor;

    public bool FogEnabled;
    public Color FogColor;
    public FogMode FogMode;
    public float FogDensity;
    public float FogStartDistance;
    public float FogEndDistance;
}

// AI: Lives in the scene and holds references you wire in the Inspector.
[ExecuteAlways]
public sealed class EnvironmentalLightingSelector : MonoBehaviour
{
    // AI: Assign the controller roots (or individual objects) you want to toggle.
    [SerializeField] public GameObject MatthewsController;
    [SerializeField] public GameObject AddysController;
    [SerializeField] public GameObject PureDarknessController;

    // AI: Optional per-preset skyboxes and ambient settings.
    [Header("Matthew's Preset")]
    [SerializeField] public EnvironmentalLightingSettings MatthewsSettings;

    [Header("Addy's Preset")]
    [SerializeField] public EnvironmentalLightingSettings AddysSettings;

    [Header("Pure Darkness Preset")]
    [SerializeField] public EnvironmentalLightingSettings PureDarknessSettings;
}
