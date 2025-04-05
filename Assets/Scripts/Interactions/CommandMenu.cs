using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public GameObject panel;
    public Button stayButton;
    public Button followButton;

    private SonNPC currentNPC;

    public void Open(SonNPC npc)
    {
        currentNPC = npc;
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        currentNPC = null;
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
