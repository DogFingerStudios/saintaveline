using UnityEngine;
using TMPro;

public class DebugHUD : MonoBehaviour
{
    public GameObject hudPanel;
    public TextMeshProUGUI groundedText;
    public CharacterController controller;

    public SonNPC sonNPC;
    public TextMeshProUGUI sonNPCStateText;

    private bool isVisible = true;

    void Update()
    {
        // Toggle visibility with "."
        if (Input.GetKeyDown(KeyCode.Period))
        {
            isVisible = !isVisible;
            hudPanel.SetActive(isVisible);
        }

        if (isVisible)
        {
            groundedText.text = "Grounded: " + controller.isGrounded.ToString();
            sonNPCStateText.text = "Son NPC State: " + sonNPC.StateMachine.CurrentState?.GetType().Name;
        }
    }
}
