using UnityEngine;

public class PosterSwap : MonoBehaviour
{
    [Header("Poster Textures")]
    [SerializeField] private Texture _originalTexture;
    [SerializeField] private Texture _alternateTexture;
    
    [Header("AI: Swap Cooldown Settings")]
    [SerializeField] private float _swapCooldownMinutes = 0.25f; 

    private Material _material;
    private float _lastSwapTime = -999f; // AI: Time when last swap occurred

    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _originalTexture = _material.mainTexture;
    }

    public void LookedAwayHandler()
    {
        if (_material.mainTexture == _alternateTexture)
        {
            _material.mainTexture = _originalTexture;
            return;
        }

        float currentTime = Time.time;
        float timeSinceLastSwap = currentTime - _lastSwapTime;
        float cooldownInSeconds = _swapCooldownMinutes * 60f;

        if (timeSinceLastSwap < cooldownInSeconds) return;

        _lastSwapTime = currentTime;
        _material.mainTexture = _alternateTexture;
    }
}
