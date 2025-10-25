using TMPro;
using UnityEngine;

public class GamePlayHUD : MonoBehaviour
{    
    public GameObject player;
    public TextMeshProUGUI healthText;
    private PlayerStats playerStats;

    public SonNPC sonNPC;
    public TextMeshProUGUI sonHealthText;


    void Start()
    {
        playerStats = player.GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || healthText == null) return;
        if (playerStats != null)
        {
            healthText.text = "Health: " + playerStats.Health.ToString() + "/" + playerStats.MaxHealth.ToString();
        }

        if (sonNPC == null) return;
        sonHealthText.text = "Son Health: " + sonNPC.Health.ToString() + "/" + sonNPC.MaxHealth.ToString();
    }
}