using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private Image _fillImage;

    private float _maxHealth;

    private readonly Color _fullColor = Color.green;
    private readonly Color _zeroColor = Color.red;

    private void Awake()
    {
        var iHasHealth = _targetObject.GetComponent<IHasHealth>();
        if (iHasHealth == null)
        {
            throw new System.Exception("Target object does not implement IHasHealth interface.");
        }
        
        _maxHealth = iHasHealth.MaxHealth;
        iHasHealth.OnHealthChanged += UpdateBar;
        UpdateBar(iHasHealth.Health);
    }

    private void UpdateBar(float health)
    {
        float t = health / _maxHealth;

        _fillImage.rectTransform.localScale = new Vector3(t, 1f, 1f);
        _fillImage.color = Color.Lerp(_zeroColor, _fullColor, t);
    }
}
