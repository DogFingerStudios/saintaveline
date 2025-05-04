using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _maxHealth = 100f;

    private float _currentHealth;
    public float CurrentHealth => _currentHealth;

    private readonly Color _fullColor = Color.green;
    private readonly Color _zeroColor = Color.red;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        UpdateBar();
    }

    public void SetHealth(float value)
    {
        _currentHealth = Mathf.Clamp(value, 0f, _maxHealth);
        UpdateBar();
    }

    private void UpdateBar()
    {
        float t = _currentHealth / _maxHealth;

        // AI: Shrink the fill horizontally
        _fillImage.rectTransform.localScale = new Vector3(t, 1f, 1f);

        // AI: Lerp fill color from red to green
        _fillImage.color = Color.Lerp(_zeroColor, _fullColor, t);
    }
}
