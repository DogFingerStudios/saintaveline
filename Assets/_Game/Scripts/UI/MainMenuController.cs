using UnityEngine;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] public GameObject continueButton = null!;

    private string _saveDirectoryPath = null!;

    void Awake()
    {
        _saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Saves");

        if (!Directory.Exists(_saveDirectoryPath))
        {
            Directory.CreateDirectory(_saveDirectoryPath);
        }

        Debug.Log($"Save directory path: {_saveDirectoryPath}");
    }

    void Start()
    {
        Debug.Log($"Number of files in save directory: {Directory.GetFiles(_saveDirectoryPath).Length}");
        if (Directory.GetFiles(_saveDirectoryPath).Length > 0)
        {
            continueButton.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // void Update()
    // {
        
    // }
}
