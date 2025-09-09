using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInventory : MonoBehaviour
{
    // ScrollView serialized field
    [SerializeField]
    private GameObject _scrollView;

    private List<ItemEntity> _items = new List<ItemEntity>();
    // public List<ItemEntity> Items { get => _items; }

    public Transform contentPanel; // Reference to the Content object in ScrollView
    public GameObject itemPrefab; // Reference to the item UI template (e.g., Button)
    private List<GameObject> itemObjects = new List<GameObject>(); // Track instantiated items

    void Start()
    {
        if (_scrollView == null)
        {
            throw new System.Exception("ScrollView GameObject is not assigned in the inspector.");
        }

        // Initially hide the scroll view
        _scrollView.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            _scrollView.gameObject.SetActive(!_scrollView.gameObject.activeSelf);
            if (!_scrollView.gameObject.activeSelf) return;

            foreach (var item in _items)
            {
                Debug.Log($"Item: {item.name}");
            }
        }
    }

    private void RefreshInventory()
    {
        // Clear existing UI items
        foreach (GameObject item in itemObjects)
        {
            Destroy(item);
        }
        itemObjects.Clear();

        // Instantiate new UI items
        foreach (var item in _items)
        {
            GameObject newItem = Instantiate(itemPrefab, contentPanel);
            newItem.SetActive(true); // Ensure the item is visible

            // Set item name (assumes TextMeshPro or Text component in the prefab)
            TextMeshProUGUI text = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = item.ItemData.ItemName;
            }

            // Add selection behavior
            Button button = newItem.GetComponent<Button>();
            if (button != null)
            {
                // button.onClick.AddListener(() => OnItemSelected(itemName));
            }

            itemObjects.Add(newItem);
        }
    }

    public void AddItem(ItemEntity item)
    {
        _items.Add(item);
        RefreshInventory();
    }
}
