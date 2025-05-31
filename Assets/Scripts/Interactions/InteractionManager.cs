using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;
using System;

[System.Serializable]
public struct InteractionData
{
    public string key;
    public string description;
}

// This script is attached to the `InteractMenus` canvas in the Hierarchy. `InteractMenus` is the parent
// of all the interact menus in the game. This class is responsible for opening and closing the interact
// menu, acreating the buttons, and handling button clicks
public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private GameObject _buttonPanel;
    [SerializeField] private GameObject crossHair;
    [SerializeField] private GameObject helpText;

    // define a callback that callers can use to execute the action
    public event Action<string> OnInteractionAction;
    public event Action OnLateInteractionAction;

    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public void OpenMenu(List<InteractionData> interactions)
    {
        // Check if the menu is already open to prevent repeated button spawning
        if (_buttonPanel.activeInHierarchy)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _buttonPanel.SetActive(true);
        crossHair.SetActive(false);
        helpText.SetActive(false);

        foreach (var interaction in interactions)
        {
            GameObject buttonObj = Instantiate(_buttonPrefab, _buttonPanel.transform);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = interaction.description;
            }

            if (buttonObj.TryGetComponent<Button>(out var button))
            {
                button.onClick.AddListener(() => OnInteractionClicked(interaction.key));
            }
        }
    }

    private void CloseMenu()
    {
        foreach (Transform child in _buttonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        helpText.SetActive(true);
        crossHair.SetActive(true);
        _buttonPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        OnInteractionAction = null;
    }

    private void OnInteractionClicked(string action)
    {
        OnInteractionAction?.Invoke(action);
        this.CloseMenu();
        OnLateInteractionAction?.Invoke();
    }

    void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // nothing to do
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }
}
