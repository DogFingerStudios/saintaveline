using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDebugHUD : MonoBehaviour
{
    public BaseNPC NPC;

    [Header("Debug Health Slider")]
    private Transform _playerTransform;
    public Slider HealthSlider;
    public TextMeshProUGUI DistanceText;


    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (_playerTransform == null)
        {
            Debug.LogError("Player transform not found in the scene.");
            return;
        }

        if (NPC == null)
        {
            Debug.LogError("NPC was null. Assign it in the inspector.");
            return;
        }

        SetUpHealthSlider();
    }

    private void SetUpHealthSlider()
    {
        if (HealthSlider == null)
        {
            Debug.LogError("HealthSlider not assigned on NPC: " + name);
            return;
        }

        HealthSlider.minValue = 0;
        HealthSlider.maxValue = NPC.MaxHealth;
        HealthSlider.value = NPC.Health;
    }

    private void LateUpdate()
    {
        if (HealthSlider != null && NPC != null)
        {
            HealthSlider.value = NPC.Health;
        }

        if (DistanceText != null)
        {
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            DistanceText.text = $"{distance:F2} m";
        }
    }

}