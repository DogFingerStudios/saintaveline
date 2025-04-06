using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public GameObject panel;
    public GameObject crossHair;
    public Button stayButton;
    public Button followButton;

    private SonNPC currentNPC;

    public void Open(SonNPC npc)
    {
        currentNPC = npc;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        panel.SetActive(true);
        crossHair.SetActive(false);
        Debug.Log("Command Menu Opened");
    }

    public void Close()
    {
        crossHair.SetActive(true);
        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentNPC = null;
        Debug.Log("Command Menu Closed");
    }

    void Start()
    {
        panel.SetActive(true);

        stayButton.onClick.AddListener(() =>
        {
            // currentNPC?.SetStateIdle();
            Close();
        });

        followButton.onClick.AddListener(() =>
        {
            // currentNPC?.SetStateFollow();
            Close();
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
