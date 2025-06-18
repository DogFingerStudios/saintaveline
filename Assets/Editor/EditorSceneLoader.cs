using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorSceneLoader : Editor
{
    /// <summary>
    /// Paths of all important scenes.
    /// </summary>
    private static readonly string SplashScreen = "Assets/Scenes/SplashScreen";
    private static readonly string MainMenu = "Assets/Scenes/MainMenu";
    private static readonly string GameOver = "Assets/Scenes/GameOver";
    private static readonly string Game = "Assets/Scenes/Game";


    // ***** Main Scenes ***** //
    // ---------------------- //
    [MenuItem("Scene Selector/SplashScreen", false, 0)]
    public static void OpenSplashScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(SplashScreen + ".unity");
    }

    [MenuItem("Scene Selector/MainMenu", false, 1)]
    public static void OpenMainMenuScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(MainMenu + ".unity");
    }

    [MenuItem("Scene Selector/GameOver", false, 2)]
    public static void OpenGameOverScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(GameOver + ".unity");
    }

    [MenuItem("Scene Selector/Game", false, 3)]
    public static void OpenGameScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(Game + ".unity");
    }

}