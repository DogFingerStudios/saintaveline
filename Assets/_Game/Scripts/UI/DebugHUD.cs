#nullable enable
using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    [SerializeField] private GameObject _debugPanel;

    [Header("Object to measure distance to")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private GameObject distanceObject;

    private Transform _playerTransform;
    
    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Toggle visibility with "."
        if (Input.GetKeyDown(KeyCode.Period))
        {
            _debugPanel.SetActive(!_debugPanel.activeSelf);
        }

        if (!_debugPanel.activeSelf) return;

        float distanceValue = Vector3.Distance(_playerTransform.position, distanceObject.transform.position);
        distanceText.text = "Distance: " + distanceValue.ToString("F2");
    }
}