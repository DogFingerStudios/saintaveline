using System;
using UnityEngine;

public class UIManagerState : IDisposable
{
    private UIManager _manager;
    private bool _crosshairActive = false;
    private CursorLockMode _cursorLockMode;
    private bool _cursorVisible;

    public UIManagerState(UIManager manager)
    {
        _manager = manager;
        _crosshairActive = _manager.CrossHair.activeSelf;
        _cursorLockMode = Cursor.lockState;
        _cursorVisible = Cursor.visible;
    }
    
    public void Dispose()
    {
        _manager.CrossHair.SetActive(_crosshairActive);
        Cursor.lockState = _cursorLockMode;
        Cursor.visible = _cursorVisible;
    }
}

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _crossHair;
    public GameObject CrossHair => _crossHair;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public UIManagerState SetState(bool crossHairVisible, CursorLockMode cursorLockMode, bool cursorVisible)
    {
        var retval = PushState();

        _crossHair.SetActive(crossHairVisible);
        Cursor.lockState = cursorLockMode;
        Cursor.visible = cursorVisible;

        return retval;
    }

    public UIManagerState PushState()
    {
        UIManagerState retval = new(this);
        return retval;
    }
}
