using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour 
{
    public void SetLoadFilename(string saveFileName = "")
    {
        GameStateManager.StartMode = GameStartMode.LoadGame;
        GameStateManager.SaveFileName = saveFileName;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
