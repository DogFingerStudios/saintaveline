using System;
using UnityEngine;

public class InputManagerState : IDisposable
{
    private InputManager _manager;
    private bool _crosshairActive = false;
    private CursorLockMode _cursorLockMode;
    private bool _cursorVisible;

    public InputManagerState(InputManager manager)
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

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject _crossHair;
    public GameObject CrossHair => _crossHair;

    public static InputManager Instance { get; private set; }

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

    public InputManagerState SetState(bool crossHairVisible, CursorLockMode cursorLockMode, bool cursorVisible)
    {
        var retval = PushState();

        _crossHair.SetActive(crossHairVisible);
        Cursor.lockState = cursorLockMode;
        Cursor.visible = cursorVisible;

        return retval;
    }

    public InputManagerState PushState()
    {
        InputManagerState retval = new(this);
        return retval;
    }
}
