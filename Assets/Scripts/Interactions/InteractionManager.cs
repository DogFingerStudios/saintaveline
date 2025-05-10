using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject _buttonPrefab; 
    [SerializeField] private GameObject _buttonPanel;
    public GameObject crossHair;
    public GameObject helpText;

    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public void DoIt()
    {
        Debug.Log("DoIt called");
    }

    public void OpenMenu(List<string> interactions)
    {
        Debug.Log("OpenMenu called");
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        _buttonPanel.SetActive(true);
        crossHair.SetActive(false);
        helpText.SetActive(false);

        foreach (var interaction in interactions)
        {
            Debug.Log($"Interaction: {interaction}");
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
    }

    private void CloseMenu()
    {
        Debug.Log("CloseMenu called");
        
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
        Debug.Log($"Clicked on interaction: {action}");
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
