using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

public class ConifersInstaller : EditorWindow
{
    static ConifersInstaller window;

    int selectedPipeline = 0;
    string launcherPath = "Assets/Forst/Conifers [BOTD]/Installer/Editor/LaunchConifersInstaller";
    string[] supportedPipelines = new string[3]
    {
        "Built in render pipeline",
        "URP",
        "HDRP"
    };
    string[] packagePaths = new string[3]
    {
        "Assets/Forst/Conifers [BOTD]/Conifers_BIRP.unitypackage",
        "Assets/Forst/Conifers [BOTD]/Conifers_URP.unitypackage",
        "Assets/Forst/Conifers [BOTD]/Conifers_HDRP.unitypackage"
    };
    bool pipelineChecked = false;

    [MenuItem( "Window/Forst/Install Conifers Package", false, 1000 )]
    public static void Init()
    {
        window = GetWindow<ConifersInstaller>(false, "Forst - Conifers Installer", true);
        window.minSize = window.maxSize = new Vector2(520, 280);
    }

    public void OnGUI()
    {
        var _style_bodytxt = new GUIStyle(EditorStyles.label);
        _style_bodytxt.wordWrap = true;
        _style_bodytxt.fontSize = 12;

        if (!pipelineChecked)
        {
            pipelineChecked = true;
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("HD"))
                {
                    selectedPipeline = 2;
                }
                else if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("Universal"))
                {
                    selectedPipeline = 1;
                }
            }
        }

        GUILayout.Space(16);

        EditorGUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.LabelField("Welcome to the Conifers [BOTD] Package!", EditorStyles.boldLabel);
            GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.LabelField(
                "This package contains multiple sub packages which add support for the built in render pipeline, URP or HDRP. " + 
                "Please select the render pipeline used in your project to install the appropriate sub package.", _style_bodytxt);
            GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.Label("Choose your render pipeline", GUILayout.Width(240));
            GUILayout.BeginVertical();
                selectedPipeline = EditorGUILayout.Popup(selectedPipeline, supportedPipelines);
                GUILayout.Space(4);
                if (GUILayout.Button("Install selected package", GUILayout.Height(28)))
                {
                    InstallPackage();
                }
            GUILayout.EndVertical();
            GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.Space(16);

        GUI.enabled = true; 
        if (!File.Exists(launcherPath + ".cs"))
        {
            GUI.enabled = false;    
        }

        GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.LabelField(
                "Once you have installed the needed package you can remove the automatic launcher script from your project. " + 
                "You can still open this window by choosing 'Window' -> 'Forst' -> 'Install Conifers Package'.", _style_bodytxt);
            GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.Label("Delete the launcher", GUILayout.Width(240));
            if (GUILayout.Button("Delete", GUILayout.Height(28)))
            {
                FileUtil.DeleteFileOrDirectory(launcherPath + ".cs");
                FileUtil.DeleteFileOrDirectory(launcherPath + ".meta");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void InstallPackage()
    {
        AssetDatabase.ImportPackage(packagePaths[selectedPipeline], true);
    } 
}
