#nullable enable

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using Image = UnityEngine.UI.Image;

// This script is attached to the Inventory UI dialog prefab. 
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

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

    private List<GameObject> _itemObjects = new List<GameObject>(); // Track instantiated items

    // used to preserve the state of the crosshair, cursor lock mode,
    // and cursor visibility
    private InputManagerState? _inputState = null;

    public bool IsActive => _inventoryDlg.activeSelf;

    private int _selectedCount = 0;

    private CharacterEntity? _owner = null;

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
            if (!InventoryUI.Instance.IsActive) return;
            CloseDialog();
        }
    }

    public void ShowInventory(CharacterEntity entity)
    {
        _inputState = InputManager.Instance.PushState();
        InputManager.Instance.SetState(false, CursorLockMode.None, true);

        foreach (GameObject item in _itemObjects)
        {
            Destroy(item);
        }
        _itemObjects.Clear();

        foreach (ItemEntity item in entity.Inventory)
        {
            if (item.ItemData == null) continue;

            GameObject newItem = Instantiate(_itemPrefab, _contentPanel);
            newItem.SetActive(true);

            TextMeshProUGUI text = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = item.ItemData.ItemName;
            }

            InventoryItemHelper helper = newItem.GetComponentInChildren<InventoryItemHelper>();
            if (helper != null && helper.Thumbnail != null && item.ItemData.Thumbnail != null)
            {
                helper.Thumbnail.sprite = item.ItemData.Thumbnail;
                helper.ItemEntity = item;
            }

            Toggle itemToggle = newItem.GetComponentInChildren<Toggle>();
            if (itemToggle != null)
            {
                itemToggle.isOn = false;
                itemToggle.onValueChanged.AddListener(isOn
                    => OnToggleClicked(item.ItemData.ItemName, isOn));
            }

            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemClicked(newItem, item.ItemData.ItemName));
            }

            _itemObjects.Add(newItem);
        }

        _selectedCount = 0;
        _owner = entity;
        _inventoryDlg.SetActive(true);
    }

    private void OnToggleClicked(string itemName, bool isOn)
    {
        _selectedCount += isOn ? 1 : -1;
        SetButtonsState();
    }

    private void OnItemClicked(GameObject itemobj, string itemName)
    {
        Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();
        itemToggle.isOn = !itemToggle.isOn;
        SetButtonsState();
    }

    private void OnEquipButtonClicked()
    {
        if (_selectedCount != 1) return;
        foreach (GameObject itemobj in _itemObjects)
        {
            Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();
            if (itemToggle != null && itemToggle.isOn)
            {
                var tag = itemobj.GetComponent<InventoryItemHelper>();
                if (tag != null && tag.ItemEntity != null && _owner != null)
                {
                    _owner.SetEquippedItem(tag.ItemEntity);
                    CloseDialog();
                    return;
                }
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

    private void OnDropButtonClicked()
    {
        if (_selectedCount != 1) return;
        foreach (GameObject itemobj in _itemObjects)
        {
            Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();
            if (itemToggle != null && itemToggle.isOn)
            {
                var tag = itemobj.GetComponent<InventoryItemHelper>();
                if (tag != null && tag.ItemEntity != null && _owner != null)
                {
                    _owner.DropItem(tag.ItemEntity);
                    CloseDialog();
                    return;
                }
            }
        }
    }

    private void CloseDialog()
    {
        _inputState?.Dispose();
        _inventoryDlg.SetActive(false);
        _owner = null;
    }

    private void SetButtonsState()
    {
        _equipButton.interactable = (_selectedCount == 1);
        _useButton.interactable = (_selectedCount == 1);
        _dropButton.interactable = (_selectedCount > 0);
        _transferButton.interactable = (_selectedCount > 0);
    }

}