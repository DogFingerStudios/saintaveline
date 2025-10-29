using UnityEngine;
using UnityEngine.UI;

public class GoalIconController : MonoBehaviour
{
    [Tooltip("The UI version of this sprite to be used on the minimap.")]
    public GameObject GoalIconPrefab;

    [Tooltip("Offset radius for the minimap icon from the edge of the minimap.")]
    public float MinimapRadiusOffset = 0f;

    private Camera _minimapCamera;
    private RectTransform _minimapParent;
    private SpriteRenderer _goalIconInstance;

    private GameObject _goalIconMinimapInstance;
    private RectTransform _minimapGoalIconRect;
    private float _minimapTransformRadius;

    // AI: Pulsating effect variables
    private Color _originalColor;
    private Color _pulseColor = Color.white * 0.99f; // AI: Close to white for pulse effect
    private float _pulseSpeed = 5f; // AI: Speed of the pulsating animation
    private Image _goalIconImage;


    public void SetData(Camera minimapCamera, RectTransform minimapParent)
    {
        _minimapCamera = minimapCamera;
        _minimapParent = minimapParent;

        // Calculate the minimap radius based on the RectTransform size
        _minimapTransformRadius = _minimapParent.rect.width / 2f;
        _minimapTransformRadius += MinimapRadiusOffset;

        // Grab the world space goal icon
        _goalIconInstance = GetComponent<SpriteRenderer>();

        // Spawn the constrained minimap icon
        _goalIconMinimapInstance = Instantiate(GoalIconPrefab, _minimapParent.position, Quaternion.identity, _minimapParent);
        _goalIconImage = _goalIconMinimapInstance.GetComponent<Image>();
        _goalIconImage.sprite = _goalIconInstance.sprite;
        _goalIconImage.color = _goalIconInstance.color;
        
        // AI: Store the original color for pulsating effect
        _originalColor = _goalIconInstance.color;

        // Get the RectTransform for positioning
        _minimapGoalIconRect = _goalIconMinimapInstance.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        // Update the position of the minimap icon to stay within bounds of the minimap radius
        if (_goalIconMinimapInstance == null) return;

        // AI: Apply pulsating effect to the icon color
        float pulseValue = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f; // AI: Oscillates between 0 and 1
        Color currentColor = Color.Lerp(_originalColor, _pulseColor, pulseValue);
        _goalIconImage.color = currentColor;

        Vector3 viewportPos = _minimapCamera.WorldToViewportPoint(transform.position);

        // Convert viewport position to minimap position
        Vector2 minimapPos = new(
            (viewportPos.x - _minimapParent.pivot.x) * _minimapParent.rect.width,
            (viewportPos.y - _minimapParent.pivot.y) * _minimapParent.rect.height
        );

        // Calculate the distance from the center
        float distanceFromCenter = minimapPos.magnitude;

        // If the icon is outside the minimap radius, clamp it
        if (distanceFromCenter > _minimapTransformRadius)
        {
            minimapPos = minimapPos.normalized * _minimapTransformRadius;
        }

        _minimapGoalIconRect.anchoredPosition = minimapPos;
    }

    void OnDisable()
    {
        // Destroy the constrained minimap icon
        if (_goalIconMinimapInstance != null)
        {
            Destroy(_goalIconMinimapInstance);
        }
    }

}