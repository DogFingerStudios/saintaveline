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
        panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crossHair.SetActive(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        currentNPC = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crossHair.SetActive(true);
    }

    void Start()
    {
        panel.SetActive(false);

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
}
