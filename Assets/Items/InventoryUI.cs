using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

using Button = UnityEngine.UI.Button;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; } = null;

    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _transferButton;
    [SerializeField] private Button _closeButton;

    [SerializeField]
    private GameObject _inventoryDlg;
    
    [SerializeField]
    private Transform _contentPanel; // Reference to the Content object in ScrollView

    [SerializeField]
    private GameObject _itemPrefab; // Reference to the item UI template (e.g., Button)

    private List<GameObject> itemObjects = new List<GameObject>(); // Track instantiated items

    // used to preserve the state of the crosshair, cursor lock mode,
    // and cursor visibility
    private InputManagerState? _inputState = null;

    public bool IsActive => _inventoryDlg.activeSelf;

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

        _inventoryDlg.SetActive(false);

        _equipButton.onClick.AddListener(() => OnEquipButtonClicked());
        _useButton.onClick.AddListener(() => OnUseButtonClicked());
        _transferButton.onClick.AddListener(() => OnTransferButtonClicked());
        _closeButton.onClick.AddListener(() => CloseDialog());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!InventoryUI.Instance.IsActive) return;
            CloseDialog();
        }
    }

    public void ShowInventory(CharacterEntity entity)
    {
        _inputState = InputManager.Instance.PushState();
        InputManager.Instance.SetState(false, CursorLockMode.None, true);

        foreach (GameObject item in itemObjects)
        {
            Destroy(item);
        }
        itemObjects.Clear();

        foreach (ItemEntity item in entity.Inventory)
        {
            GameObject newItem = Instantiate(_itemPrefab, _contentPanel);
            newItem.SetActive(true);

            TextMeshProUGUI text = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = item.ItemData.ItemName;
            }

            var tag = newItem.GetComponent<InventoryItemEntityTag>();
            if (tag != null) tag.ItemEntity = item;

            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemSelected(item.ItemData.ItemName));
            }

            itemObjects.Add(newItem);
        }

        _inventoryDlg.SetActive(true);
    }

    private void OnItemSelected(string itemName)
    {
        Debug.Log("Selected item: " + itemName);
    }

    private void OnEquipButtonClicked()
    {
        // list all the items in the _contentPanel
        foreach (Transform child in _contentPanel)
        {
            var tag = child.GetComponent<InventoryItemEntityTag>();
            if (tag != null && tag.ItemEntity != null)
            {
                Debug.Log("Equip button clicked for item: " + tag.ItemEntity.ItemData.ItemName);
                // Here you can add logic to equip the item
                break; // Equip the first item found for demonstration
            }
        }
    }

    private void OnUseButtonClicked()
    {
        Debug.Log("Use button clicked");
    }

    private void OnTransferButtonClicked()
    {
        Debug.Log("Transfer button clicked");
    }

    private void CloseDialog()
    {
        _inputState?.Dispose();
        _inventoryDlg.SetActive(false);
    }
}
