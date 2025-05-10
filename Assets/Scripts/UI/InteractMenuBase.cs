using UnityEngine;

public class InteractMenuBase : MonoBehaviour
{
    public GameObject panel;

    [Header("External Widgets")]
    [Tooltip("These widgets are disabled when the menu is open")]
    public GameObject crossHair;
    public GameObject helpText;

    public virtual void Open()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        panel.SetActive(true);
        crossHair.SetActive(false);
        helpText.SetActive(false);
    }

    public virtual void Close()
    {
        helpText.SetActive(true);
        crossHair.SetActive(true);
        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected virtual void Start()
    {
        panel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
