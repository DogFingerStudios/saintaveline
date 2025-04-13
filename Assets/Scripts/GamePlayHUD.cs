using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayHUD : MonoBehaviour
{
    public GameObject player;
    public TextMeshProUGUI healthText; 

    public SonNPC sonNPC;
    public TextMeshProUGUI sonHealthText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || healthText == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            healthText.text = "Health: " + stats.health.ToString() + "/" + stats.maxHealth.ToString();
        }
        if (sonNPC == null) return;
        sonHealthText.text = "Son Health: " + sonNPC.Health.ToString() + "/" + sonNPC.MaxHealth.ToString();
    }
}
