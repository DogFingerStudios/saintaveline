using UnityEngine;

public class PosterSwap : MonoBehaviour
{
    [SerializeField] private Texture _originalTexture;
    [SerializeField] private Texture _alternateTexture;

    private bool _originalShowing = true;
    private Material _material;

    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _originalTexture = _material.mainTexture;
    }

    public void LookedAwayHandler()
    {
        _originalShowing = !_originalShowing;
        if (_originalShowing)
        {
            _material.mainTexture = _originalTexture;
        }
        else
        {
            _material.mainTexture = _alternateTexture;
        }
    }
}
