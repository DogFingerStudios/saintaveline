#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DependencyViewer
{
    [MenuItem("Tools/Show Prefab Dependencies")]
    public static void ShowDependencies()
    {
        var selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("No asset selected.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning("Selected asset has no valid path.");
            return;
        }

        string[] dependencies = AssetDatabase.GetDependencies(assetPath, true); // true = recursive

        Debug.Log($"Dependencies for {assetPath}:");
        foreach (var dep in dependencies)
        {
            Debug.Log(dep);
        }
    }
}
#endif
