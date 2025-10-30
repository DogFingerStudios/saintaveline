using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public string targetScene = "MainMenu";


    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(targetScene);
        }
    }

}