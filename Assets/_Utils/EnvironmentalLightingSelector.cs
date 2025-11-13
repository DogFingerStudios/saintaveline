using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class EnvironmentalLightingSettings
{
    [SerializeField] public Material SkyboxMaterial;
    public Light SunSource;
    public Color RealtimeShadowColor;
    
    public AmbientMode EnvironmentLightingSource;
    [ColorUsage(true, true)] public Color EnvironmentLightingSkyColor;
    [ColorUsage(true, true)] public Color32 EnvironmentLightingEquatorColor;
    [ColorUsage(true, true)] public Color32 EnvironmentLightingGroundColor;

    public bool FogEnabled;
    public Color FogColor;
    public FogMode FogMode;
    public float FogDensity;
    public float FogStartDistance;
    public float FogEndDistance;
}

[ExecuteAlways]
public class EnvironmentalLightingSelector : MonoBehaviour
{
    // AI: Assign the controller roots (or individual objects) you want to toggle.
    [SerializeField] public GameObject MatthewsController;
    [SerializeField] public GameObject AddysController;
    [SerializeField] public GameObject PureDarknessController;

    [Header("Matthew's Day Night Controller Settings")]
    [SerializeField] public EnvironmentalLightingSettings MatthewsSettings;
    
    [Header("Addy's Day Night Controller Settings")]
    [SerializeField] public EnvironmentalLightingSettings AddysSettings;

    [Header("Pure Darkness Settings")]
    [SerializeField] public EnvironmentalLightingSettings PureDarknessSettings;
}
