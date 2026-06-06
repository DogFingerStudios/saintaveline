using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script is attached to the prefab `NPCDebugCanvas` which should be attached directly under
// a GameObject that has a script derived from `BaseNPC` attached to it. As of the this writing
// there is only `EnemyNPC` and `FriendlyNPC`, but any script/class which derives from `BaseNPC`
// should work
public class NPCDebugHUD : MonoBehaviour
{
    private Transform _playerTransform;
    private BaseNPC _thisNPC;

    public Slider HealthSlider;
    public TextMeshProUGUI DistanceText;
    public TextMeshProUGUI StateText;
    public TextMeshProUGUI NameText;

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (_playerTransform == null)
        {
            Debug.LogError("Player transform not found in the scene.");
        }

        foreach (var component in transform.parent.GetComponents<Component>())
        {
            if (component is BaseNPC npcComponent && npcComponent.enabled)
            {
                _thisNPC = npcComponent;
                break;
            }
        }

        if (_thisNPC == null)
        {
            Debug.LogError("Parent GameObject does not have a `BaseNPC` compatible component attached.");
        }

        SetUpHealthSlider();
        NameText.text = _thisNPC.Name;
    }

    private void SetUpHealthSlider()
    {
        if (HealthSlider == null)
        {
            Debug.LogError("HealthSlider not assigned on NPC: " + name);
            return;
        }

        HealthSlider.minValue = 0;
        HealthSlider.maxValue = _thisNPC.MaxHealth;
        HealthSlider.value = _thisNPC.Health;
    }

    private void LateUpdate()
    {
        if (_thisNPC == null || _playerTransform == null) return;

        if (HealthSlider != null)
        {
            HealthSlider.value = _thisNPC.Health;
        }

        if (DistanceText != null)
        {
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            DistanceText.text = $"{distance:F2} m";
        }

        if (StateText != null)
        {
            var currentState = _thisNPC.StateMachine?.CurrentState;
            StateText.text = currentState?.GetType().Name ?? "<Unknown State>";
        }

        NameText.color = _thisNPC.IsAggro ? Color.red : Color.green;
    }
}