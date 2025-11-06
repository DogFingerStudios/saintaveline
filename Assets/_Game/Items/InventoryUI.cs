#nullable enable

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

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
    [SerializeField] private Canvas _inventoryDlg;
    [SerializeField] private Transform _contentPanel;   // Reference to the Content object in ScrollView
    [SerializeField] private GameObject _itemPrefab;    // Reference to the item UI template (e.g., Button)

    private List<GameObject> _itemObjects = new List<GameObject>(); // Track instantiated items

    // used to preserve the state of the crosshair, cursor lock mode,
    // and cursor visibility
    private UIManagerState? _inputState = null;

    public bool IsActive => _inventoryDlg.enabled;

    private CharacterEntity? _owner = null;

    private int _selectedCount = 0;
    private Dictionary<string, CharacterEntity> _transferTargets = new Dictionary<string, CharacterEntity>();
    private float _maxTransferDistance = 7.5f;

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

        //_inventoryDlg.enabled = false;

        _equipButton.onClick.AddListener(() => OnEquipButtonClicked());
        _equipButton.interactable = false;

        _useButton.onClick.AddListener(() => OnUseButtonClicked());
        _useButton.interactable = false;

        _dropButton.onClick.AddListener(() => OnDropButtonClicked());
        _dropButton.interactable = false;

        _transferButton.onClick.AddListener(() => OnTransferButtonClicked());
        _transferButton.interactable = false;

        _closeButton.onClick.AddListener(() => CloseDialog());

        var listItems = new List<string>();
        foreach (var npc in GameObject.FindGameObjectsWithTag("FriendlyNPC"))
        {
            _transferTargets.Add(npc.name, npc.GetComponent<CharacterEntity>()!);
            listItems.Add(npc.name);
        }

        // add the Player
        _transferTargets.Add("Player", GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterEntity>()!);
        listItems.Add("Player");

        _transferDropdown.AddOptions(listItems.OrderBy(x => x).ToList());

        InputManager.Instance.RegisterInputHandler(InputState.InventoryDlg, this.ProcessInput);
    }

    public void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!IsActive) return;
            CloseDialog();
        }
    }

    public void ShowInventory(CharacterEntity entity)
    {
        _inputState = UIManager.Instance.SetState(false, CursorLockMode.None, true);

        foreach (GameObject item in _itemObjects)
        {
            Destroy(item);
        }
        _itemObjects.Clear();

        _owner = entity;

        foreach (ItemEntity? item in entity.Inventory.Prepend(_owner!.EquippedItem))
        {
            if (item == null || item.ItemData == null) continue;

            GameObject newItem = Instantiate(_itemPrefab, _contentPanel);
            newItem.SetActive(true);

            TextMeshProUGUI text = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = item.ItemData.ItemName;
            }

            InventoryItemHelper helper = newItem.GetComponentInChildren<InventoryItemHelper>();
            if (helper != null && helper.Thumbnail != null)
            {
                if (item.ItemData.Thumbnail != null)
                {
                    helper.Thumbnail.sprite = item.ItemData.Thumbnail;
                }
                
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

            if (item == _owner.EquippedItem)
            {
                var image = newItem.GetComponentInChildren<Image>();
                if (image != null)
                {
                    image.color = new Color(0.8f, 0.8f, 1.0f, 1.0f);
                }
            }

            _itemObjects.Add(newItem);
        }

        _selectedCount = 0;
        _inventoryDlg.enabled = true;
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
                    string msg;
                    if (IsEquippedItemSelected())
                    {
                        _owner.AddItemToInventory(tag.ItemEntity);
                        msg = $"Item '{tag.ItemEntity.ItemData!.ItemName}' unequipped.";
                    }
                    else
                    {
                        _owner.SetEquippedItem(tag.ItemEntity);
                        msg = $"Item '{tag.ItemEntity.ItemData!.ItemName}' equipped.";
                    }

                    BottomTypewriter.Instance.Enqueue(msg);
                    CodexOverlayController.Instance.ToggleCodexOverlay(false);
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
        string targetName = _transferDropdown.options[_transferDropdown.value].text;
        if (!_transferTargets.ContainsKey(targetName))
        {
            throw new System.Exception("Transfer target not found: " + targetName);
        }

        List<ItemEntity> itemsToTransfer = new List<ItemEntity>();
        foreach (GameObject itemobj in _itemObjects)
        {
            Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();
            if (itemToggle != null && itemToggle.isOn)
            {
                var tag = itemobj.GetComponent<InventoryItemHelper>();
                if (tag != null && tag.ItemEntity != null && _owner != null)
                {
                    itemsToTransfer.Add(tag.ItemEntity);
                }
            }
        }

        if (itemsToTransfer.Count == 0) return;

        var entity = _transferTargets[targetName];
        if (entity == _owner)
        {
            CloseDialog();
            BottomTypewriter.Instance.Enqueue("Cannot transfer item to self.");
            return;
        }

        if (Vector3.Distance(_owner!.transform.position, entity!.transform.position) > _maxTransferDistance)
        {
            CloseDialog();
            BottomTypewriter.Instance.Enqueue($"Cannot transfer item(s) to {targetName}: too far away.");
            return;
        }

        foreach (var itemToTransfer in itemsToTransfer)
        {
            entity.AddItemToInventory(itemToTransfer);
            _owner!.RemoveItemFromInventory(itemToTransfer);
        }

        if (itemsToTransfer.Count == 1)
        {
            BottomTypewriter.Instance.Enqueue($"Transferred '{itemsToTransfer[0].ItemData!.ItemName}' to {targetName}");
        }
        else
        {
            BottomTypewriter.Instance.Enqueue($"Transferred {itemsToTransfer.Count} items to {targetName}");
        }

        CloseDialog();
    }

    private void OnDropButtonClicked()
    {
        List<ItemEntity> droppedItems = new List<ItemEntity>();

        foreach (GameObject itemobj in _itemObjects)
        {
            Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();
            if (itemToggle != null && itemToggle.isOn)
            {
                var tag = itemobj.GetComponent<InventoryItemHelper>();
                if (tag != null && tag.ItemEntity != null && _owner != null)
                {
                    _owner.DropItem(tag.ItemEntity);
                    droppedItems.Add(tag.ItemEntity);
                }
            }
        }

        if (droppedItems.Count == 1)
        {
            BottomTypewriter.Instance.Enqueue($"Dropped '{droppedItems[0].ItemData!.ItemName}'");
        }
        else
        {
            BottomTypewriter.Instance.Enqueue($"Dropped {droppedItems.Count} items");
        }

        CloseDialog();
        return;
    }

    private void CloseDialog()
    {
        InputManager.Instance.SetInputState(InputState.Gameplay);
        _inputState?.Dispose();
        _inventoryDlg.enabled = false;
        _owner = null;
    }

    private void SetButtonsState()
    {
        _equipButton.interactable = (_selectedCount == 1);
        _useButton.interactable = (_selectedCount == 1);
        _dropButton.interactable = (_selectedCount > 0);
        _transferButton.interactable = (_selectedCount > 0);

        if (IsEquippedItemSelected())
        {
            var text = _equipButton.GetComponentInChildren<TMP_Text>();
            text!.text = "UnEquip";
        }
        else
        {
            var text = _equipButton.GetComponentInChildren<TMP_Text>();
            text.text = "Equip";
        }
    }

    private bool IsEquippedItemSelected()
    {
        if (_selectedCount == 1 && _itemObjects.Count > 0)
        {
            GameObject itemobj = _itemObjects[0];
            Toggle itemToggle = itemobj.GetComponentInChildren<Toggle>();

            if (itemToggle != null && itemToggle.isOn)
            {
                var tag = itemobj.GetComponent<InventoryItemHelper>();
                if (tag != null && tag.ItemEntity == _owner!.EquippedItem)
                {
                    return true;
                }
            }
        }

        return false;
    }
}