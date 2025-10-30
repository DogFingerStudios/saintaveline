using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SaveOnPlay
{
    private const bool _shouldSave = true; // Change this if we don't want to auto-save

    static SaveOnPlay()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private static void PlayModeStateChanged(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.ExitingEditMode:
                if (_shouldSave) EditorSceneManager.SaveOpenScenes();
                EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            break;
        }
    }

}