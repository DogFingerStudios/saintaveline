using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SaveOnPlay
{
    static SaveOnPlay()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private static void PlayModeStateChanged(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.ExitingEditMode:
                EditorSceneManager.SaveOpenScenes();
                EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            break;
        }
    }

}