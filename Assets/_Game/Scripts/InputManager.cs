using System;
using System.Collections.Generic;
using UnityEngine;

public enum InputState
{
    Gameplay,
    InventoryDlg,
    CodexDlg,
    MapLabeling,
    BoatDriving,
    PauseMenu
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputState CurrentState { get; private set; } = InputState.Gameplay;

    Dictionary<InputState, Action> _inputHandlers = new Dictionary<InputState, Action>();
    Action _currentHandler;

    InputManager()
    {
        if (Instance != null)
        {
            throw new Exception("InputManager: Multiple instances detected. InputManager is a singleton and there should only be one instance in the scene.");
        }

        Instance = this;
    }

    public void RegisterInputHandler(InputState state, Action handler)
    {
        _inputHandlers[state] = handler;
    }

    public void SetInputState(InputState newState)
    {
        CurrentState = newState;
        if (_inputHandlers.TryGetValue(CurrentState, out var newHandler))
        {
            _currentHandler = newHandler;
        }
        else
        {
            _currentHandler = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)
            && CurrentState == InputState.Gameplay)
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
        else if (Input.GetKeyDown(KeyCode.M))
        {
            if (CurrentState == InputState.Gameplay)
            {
                if (CodexOverlayController.Instance.IsActive)
                {
                    throw new Exception("InputManager: Codex Overlay is already active when trying to open it.");
                }

                var playerEntity = this.GetComponentInParent<CharacterEntity>();
                if (playerEntity == null)
                {
                    throw new System.Exception("PlayerInteractor: CharacterEntity script not found on Player object.");
                }

                this.SetInputState(InputState.CodexDlg);
                CodexOverlayController.Instance.OpenCodexOverlay(playerEntity);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Comma) 
            && CurrentState == InputState.Gameplay)
        {
            MapLabeler.Instance.Init();
            this.SetInputState(InputState.MapLabeling);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == InputState.Gameplay)
            {
                PauseMenuController.Instance.PauseGame();
                this.SetInputState(InputState.PauseMenu);
            }
            else if (CurrentState == InputState.PauseMenu)
            {
                PauseMenuController.Instance.ResumeGame();
                this.SetInputState(InputState.Gameplay);
            }
            else if (CurrentState == InputState.CodexDlg)
            {
                if (!CodexOverlayController.Instance.IsActive)
                {
                    throw new Exception("InputManager: Codex Overlay is not active when trying to close it.");
                }

                this.SetInputState(InputState.Gameplay);
                CodexOverlayController.Instance.ToggleCodexOverlay(false);
            }
        }

        _currentHandler?.Invoke();
    }
}
