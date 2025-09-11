using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; } = null;

    [SerializeField]
    private GameObject _inventoryPanel;
    
    [SerializeField]
    private Transform _contentPanel; // Reference to the Content object in ScrollView

    [SerializeField]
    private GameObject _itemPrefab; // Reference to the item UI template (e.g., Button)

    private List<GameObject> itemObjects = new List<GameObject>(); // Track instantiated items

    public bool IsActive => _inventoryPanel.activeSelf;

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

        _inventoryPanel.SetActive(false);
    }

    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
    //    }
    //}

    public void ShowInventory(CharacterEntity entity)
    {
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

            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemSelected(item.ItemData.ItemName));
            }

            itemObjects.Add(newItem);
        }

        _inventoryPanel.SetActive(true);
    }

    public void SetActive(bool bActive)
    {
        _inventoryPanel.SetActive(bActive);
    }

    private void OnItemSelected(string itemName)
    {
        Debug.Log("Selected item: " + itemName);
    }
}
