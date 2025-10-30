using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

[Preserve]
public class SceneLoader : MonoBehaviour
{
    [UsedImplicitly]
    public void LoadScene(string sceneName)
    {
        Debug.Log("<color=green>Loading scene: " + sceneName + "</color>");
        SceneManager.LoadScene(sceneName);
    }

    [UsedImplicitly]
    public void QuitGame()
    {
        Debug.Log("<color=red>Quitting game...</color>");
        Application.Quit();
    }

    private void Update()
    {
        Debug.Log("<color=blue>Holy hell wtf</color>");
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}