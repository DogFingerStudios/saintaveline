using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;
using System;

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
    // public delegate void InteractionAction(string action);
    public event Action<string> OnInteractionAction;

    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public void OpenMenu(List<string> interactions)
    {
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
                buttonText.text = interaction;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                string capturedInteraction = interaction;
                button.onClick.AddListener(() => OnInteractionClicked(capturedInteraction));
            }
        }

        // AI: Adjust the height of _buttonPanel to fit all buttons
        RectTransform panelRectTransform = _buttonPanel.GetComponent<RectTransform>();
        if (panelRectTransform != null)
        {
            float buttonHeight = _buttonPrefab.GetComponent<RectTransform>().sizeDelta.y;
            float spacing = 10f; // Adjust spacing between buttons if needed
            float totalHeight = interactions.Count * (buttonHeight + spacing) - spacing;
            panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, totalHeight);
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
    }

    private void OnInteractionClicked(string action)
    {
        OnInteractionAction?.Invoke(action);
        this.CloseMenu();
    }

    void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
