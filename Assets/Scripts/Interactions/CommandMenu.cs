using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public GameObject panel;
    public Button stayButton;
    public Button followButton;

    [Header("External Widgets")]
    [Tooltip("These widgets are disabled when the menu is open")]
    public GameObject crossHair;
    public GameObject helpText;

    private FriendlyNPC currentNPC;

    public void Open(FriendlyNPC npc)
    {
        currentNPC = npc;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        panel.SetActive(true);
        crossHair.SetActive(false);
        helpText.SetActive(false);
    }

    public void Close()
    {
        helpText.SetActive(true);
        crossHair.SetActive(true);
        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentNPC = null;
    }

    void Start()
    {
        panel.SetActive(true);

        stayButton.onClick.AddListener(() =>
        {
            currentNPC?.setState(new NPCIdleState());
            Close();
        });

        followButton.onClick.AddListener(() =>
        {
            currentNPC?.setState(new NPCFollowState());
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
