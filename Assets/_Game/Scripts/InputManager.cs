using System;
using System.Collections.Generic;
using UnityEngine;

public enum InputState
{
    Gameplay,
    InventoryDlg,
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputState CurrentState { get; private set; } = InputState.Gameplay;

    Dictionary<InputState, Action> _inputHandlers = new Dictionary<InputState, Action>();
    Action _currentHandler;

    public void RegisterInputHandler(InputState area, Action handler)
    {
        _inputHandlers[area] = handler;
    }

    public void SetInputState(InputState newState)
    {
        CurrentState = newState;
        if (_inputHandlers.TryGetValue(CurrentState, out var newHandler) && newHandler != null)
        {
            _currentHandler = newHandler;
        }
    }

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

        _inputHandlers.Add(InputState.Gameplay, null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && CurrentState != InputState.InventoryDlg)
        {
            if (InventoryUI.Instance.IsActive)
            {
                throw new Exception("InputManager: Inventory UI is already active when trying to open it.");
            }

            var playerEntity = this.GetComponentInParent<CharacterEntity>();
            if (playerEntity == null)
            {
                throw new System.Exception("PlayerInteractor: CharacterEntity script not found on Player object.");
            }

            this.SetInputState(InputState.InventoryDlg);
            InventoryUI.Instance.ShowInventory(playerEntity);
        }

        _currentHandler?.Invoke();
    }
}
