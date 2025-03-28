using UnityEngine;
using TMPro;

public class DebugHUD : MonoBehaviour
{
    public GameObject hudPanel;
    public TextMeshProUGUI groundedText;
    public CharacterController controller;
    private bool isVisible = true;

    void Update()
    {
        // Toggle visibility with "."
        if (Input.GetKeyDown(KeyCode.Period))
        {
            isVisible = !isVisible;
            hudPanel.SetActive(isVisible);
        }

        if (isVisible && groundedText != null && controller != null)
        {
            groundedText.text = "Grounded: " + controller.isGrounded.ToString();
        }
    }
}
