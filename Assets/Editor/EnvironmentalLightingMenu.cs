using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// AI: Editor-only menu that looks up scene bindings and applies changes with Undo support.
public static class EnvironmentalLightingMenu
{
    // AI: Find the bindings component, including inactive and DontDestroyOnLoad.
    private static EnvironmentalLightingSelector FindBindings()
    {
        var found = Object.FindFirstObjectByType<EnvironmentalLightingSelector>(FindObjectsInactive.Include);
        return found;
    }

    // AI: Toggle one on, others off, with Undo + scene dirty.
    private static void ActivateOnly(GameObject on, params GameObject[] off)
    {
        if (on != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(on, "Activate Controller");
            on.SetActive(true);
            EditorSceneManager.MarkSceneDirty(on.scene);
        }

        if (off != null)
        {
            for (int i = 0; i < off.Length; i++)
            {
                var go = off[i];
                if (go != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(go, "Deactivate Controller");
                    go.SetActive(false);
                    EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }
        }
    }

    // AI: Apply environment settings and mark scene dirty.
    private static void ApplyEnvironment(Material skybox, AmbientMode ambientMode, Color ambientSkyColor, float ambientIntensity)
    {
        Undo.RecordObject(RenderSettings.skybox, "Change Skybox");
        RenderSettings.skybox = skybox;

        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientIntensity = ambientIntensity;

        if (ambientIntensity <= 0.01f)
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = null;
            RenderSettings.reflectionIntensity = 0.0f;
        }
        else
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 1.0f;
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    // AI: Validators so menu items enable only when bindings exist.
    [MenuItem("Tools/Environmental Lighting/Matthew's Day Night Controller", true)]
    private static bool ValidateMatthews()
    {
        return FindBindings() != null;
    }

    [MenuItem("Tools/Environmental Lighting/Addy's Simple Day Night Controller", true)]
    private static bool ValidateAddys()
    {
        return FindBindings() != null;
    }

    [MenuItem("Tools/Environmental Lighting/Pure Darkness", true)]
    private static bool ValidateDarkness()
    {
        return FindBindings() != null;
    }

    [MenuItem("Tools/Environmental Lighting/Matthew's Day Night Controller")]
    public static void MatthewsDayNightController()
    {
        var b = FindBindings();
        if (b == null)
        {
            Debug.LogError("EnvironmentalLightingBindings not found in scene.");
            return;
        }

        ActivateOnly(b.MatthewsController, b.AddysController, b.PureDarknessController);
        //ApplyEnvironment(b.MatthewsSkybox, b.MatthewsAmbientMode, b.MatthewsAmbientSkyColor, b.MatthewsAmbientIntensity);
    }

    [MenuItem("Tools/Environmental Lighting/Addy's Simple Day Night Controller")]
    public static void AddysSimpleDayNightController()
    {
        var b = FindBindings();
        if (b == null)
        {
            Debug.LogError("EnvironmentalLightingBindings not found in scene.");
            return;
        }

        ActivateOnly(b.AddysController, b.MatthewsController, b.PureDarknessController);
        //ApplyEnvironment(b.AddysSkybox, b.AddysAmbientMode, b.AddysAmbientSkyColor, b.AddysAmbientIntensity);
    }

    [MenuItem("Tools/Environmental Lighting/Pure Darkness")]
    public static void PureDarkness()
    {
        var b = FindBindings();
        if (b == null)
        {
            Debug.LogError("EnvironmentalLightingBindings not found in scene.");
            return;
        }

        ActivateOnly(b.PureDarknessController, b.MatthewsController, b.AddysController);
        //ApplyEnvironment(b.PureDarknessSkybox, b.PureDarknessAmbientMode, b.PureDarknessAmbientSkyColor, b.PureDarknessAmbientIntensity);
    }
}
