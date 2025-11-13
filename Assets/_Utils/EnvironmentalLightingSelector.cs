using UnityEngine;

// AI: Lives in the scene and holds references you wire in the Inspector.
[ExecuteAlways]
public sealed class EnvironmentalLightingSelector : MonoBehaviour
{
    // AI: Assign the controller roots (or individual objects) you want to toggle.
    [SerializeField] private GameObject _matthewsController;
    [SerializeField] private GameObject _addysController;
    [SerializeField] private GameObject _pureDarknessController;

    // AI: Optional per-preset skyboxes and ambient settings.
    [Header("Matthew's Preset")]
    [SerializeField] private Material _matthewsSkybox;
    [SerializeField] private UnityEngine.Rendering.AmbientMode _matthewsAmbientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    [SerializeField] private Color _matthewsAmbientSkyColor = Color.white;
    [SerializeField] private float _matthewsAmbientIntensity = 1.0f;

    [Header("Addy's Preset")]
    [SerializeField] private Material _addysSkybox;
    [SerializeField] private UnityEngine.Rendering.AmbientMode _addysAmbientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    [SerializeField] private Color _addysAmbientSkyColor = Color.white;
    [SerializeField] private float _addysAmbientIntensity = 1.0f;

    [Header("Pure Darkness Preset")]
    [SerializeField] private Material _pureDarknessSkybox;
    [SerializeField] private UnityEngine.Rendering.AmbientMode _pureDarknessAmbientMode = UnityEngine.Rendering.AmbientMode.Flat;
    [SerializeField] private Color _pureDarknessAmbientSkyColor = Color.black;
    [SerializeField] private float _pureDarknessAmbientIntensity = 0.0f;

    // AI: Public getters that the editor code will use.
    public GameObject MatthewsController
    {
        get
        {
            return _matthewsController;
        }
    }

    public GameObject AddysController
    {
        get
        {
            return _addysController;
        }
    }

    public GameObject PureDarknessController
    {
        get
        {
            return _pureDarknessController;
        }
    }

    public Material MatthewsSkybox
    {
        get
        {
            return _matthewsSkybox;
        }
    }

    public UnityEngine.Rendering.AmbientMode MatthewsAmbientMode
    {
        get
        {
            return _matthewsAmbientMode;
        }
    }

    public Color MatthewsAmbientSkyColor
    {
        get
        {
            return _matthewsAmbientSkyColor;
        }
    }

    public float MatthewsAmbientIntensity
    {
        get
        {
            return _matthewsAmbientIntensity;
        }
    }

    public Material AddysSkybox
    {
        get
        {
            return _addysSkybox;
        }
    }

    public UnityEngine.Rendering.AmbientMode AddysAmbientMode
    {
        get
        {
            return _addysAmbientMode;
        }
    }

    public Color AddysAmbientSkyColor
    {
        get
        {
            return _addysAmbientSkyColor;
        }
    }

    public float AddysAmbientIntensity
    {
        get
        {
            return _addysAmbientIntensity;
        }
    }

    public Material PureDarknessSkybox
    {
        get
        {
            return _pureDarknessSkybox;
        }
    }

    public UnityEngine.Rendering.AmbientMode PureDarknessAmbientMode
    {
        get
        {
            return _pureDarknessAmbientMode;
        }
    }

    public Color PureDarknessAmbientSkyColor
    {
        get
        {
            return _pureDarknessAmbientSkyColor;
        }
    }

    public float PureDarknessAmbientIntensity
    {
        get
        {
            return _pureDarknessAmbientIntensity;
        }
    }
}
