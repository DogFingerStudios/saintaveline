using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

// This script is attached to the Inventory UI dialog prefab. 
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; } = null;

    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _dropButton;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _transferButton;
    [SerializeField] private TMP_Dropdown _transferDropdown;
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

    private int _selectedCount = 0;


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
        _equipButton.interactable = false;

        _useButton.onClick.AddListener(() => OnUseButtonClicked());
        _useButton.interactable = false;

        _dropButton.onClick.AddListener(() => OnDropButtonClicked());
        _dropButton.interactable = false;

        _transferButton.onClick.AddListener(() => OnTransferButtonClicked());
        _transferButton.interactable = false;

        _closeButton.onClick.AddListener(() => CloseDialog());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!InventoryUI.Instance.IsActive)
                return;
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
            if (tag != null)
                tag.ItemEntity = item;

            //Button button = newItem.GetComponent<Button>();
            //if (button != null)
            //{
            //    button.onClick.AddListener(() => OnItemSelected(item.ItemData.ItemName));
            //}

            Toggle itemToggle = newItem.GetComponentInChildren<Toggle>();
            if (itemToggle != null)
            {
                itemToggle.isOn = false;
                itemToggle.onValueChanged.AddListener(isOn
                    => OnItemSelected(item.ItemData.ItemName, isOn));
            }

            itemObjects.Add(newItem);
        }

        _inventoryDlg.SetActive(true);
    }

    private void OnItemSelected(string itemName, bool isOn)
    {
        _selectedCount += isOn ? 1 : -1;
        Debug.Log("Selected item: " + itemName);
        Debug.Log("Selected count: " + _selectedCount);
        _equipButton.interactable = (_selectedCount == 1);
        _useButton.interactable = (_selectedCount == 1);
        _dropButton.interactable = (_selectedCount > 0);
        _transferButton.interactable = (_selectedCount > 0);
    }

    private void OnEquipButtonClicked()
    {
        Debug.Log("Equip button clicked");
        //// list all the items in the _contentPanel
        //foreach (Transform child in _contentPanel)
        //{
        //    var tag = child.GetComponent<InventoryItemEntityTag>();
        //    if (tag != null && tag.ItemEntity != null)
        //    {
        //        Debug.Log("Equip button clicked for item: " + tag.ItemEntity.ItemData.ItemName);
        //        // Here you can add logic to equip the item
        //        break; // Equip the first item found for demonstration
        //    }
        //}
    }

    private void OnUseButtonClicked()
    {
        Debug.Log("Use button clicked");
    }

    private void OnTransferButtonClicked()
    {
        Debug.Log("Transfer button clicked");
    }

    private void OnDropButtonClicked()
    {
        Debug.Log("Drop button clicked");
    }

    private void CloseDialog()
    {
        _inputState?.Dispose();
        _inventoryDlg.SetActive(false);
    }

}