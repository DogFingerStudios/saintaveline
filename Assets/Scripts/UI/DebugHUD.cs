using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    public GameObject hudPanel;
    public TextMeshProUGUI groundedText;
    public CharacterController controller;
    public GameObject enemyNPC;
    private IHasHealth? enemyNPCHealth;

    public SonNPC sonNPC;
    public TextMeshProUGUI sonNPCStateText;
    public TextMeshProUGUI sonNPCDistanceText;
    public TextMeshProUGUI enemyHealthText; 

    private bool isVisible = true;

    void Start()
    {
        enemyNPCHealth = enemyNPC.GetComponent<IHasHealth>();
    }

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
            sonNPCStateText.text = "SonNPC State: " + sonNPC.StateMachine.CurrentState?.GetType().Name;
            float distance = Vector3.Distance(controller.transform.position, sonNPC.transform.position);
            sonNPCDistanceText.text = "SonNPC Dist: " + distance.ToString("F2");
            enemyHealthText.text = "Enemy Health: " + enemyNPCHealth.Health.ToString("F2");
        }
    }
}
