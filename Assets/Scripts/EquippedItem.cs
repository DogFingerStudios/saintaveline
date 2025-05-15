#nullable enable

using UnityEngine;

/// <summary>
/// This script is attached to the root `Player` object and handles the mechanics
/// of the equipped item (e.g. dropping, storing, weilding, etc.)
/// </summary>
public class EquippedItem : MonoBehaviour
{
    private PlayerInteractor? _interactor;
    
    [SerializeField] private Transform? _equippedItemPos;

    private ItemInteraction? _itemInteraction = null;

    private GameObject? _equippedItem;
    public GameObject? EquippedItemObject
    {
        get => _equippedItem;
        set
        {
            if (!value) 
            {
                DropEquippedItem();
                return;
            }

            _equippedItem = value;
            _equippedItem.transform.SetParent(_equippedItemPos);

            // Once parented to the item position, set the local position and rotation to zero,
            // so it matches the item pos transform.
            _equippedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Do we want to disable physics while we equip the item?
            // If item doesn't have physics, we don't need to do anything
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb)) 
            {
                rb.isKinematic = true;
            }

            _itemInteraction = _equippedItem.GetComponent<ItemInteraction>();
            if (!_itemInteraction)
            {
                Debug.LogError($"Equipped item '{_equippedItem.name}' does not have an ItemInteraction component.");
            }
        }
    } 

    public void DropEquippedItem() 
    {
        if (_equippedItem != null) 
        {
            _itemInteraction!.onUnequipped();
            // If item has physics, re-enable them
            if (_equippedItem.TryGetComponent<Rigidbody>(out var rb)) 
            {
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            _equippedItem.transform.SetParent(null);
            _equippedItem = null;
        }
    }
    
    void Awake()
    {
        _interactor = GetComponentInChildren<PlayerInteractor>();
        if (_interactor == null) 
        {
            Debug.LogError("EquippedItem script requires a PlayerInteractor component on the same GameObject.");
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (_interactor!.FocusedObject != null)
            {
                if (_equippedItem != null) 
                {
                    DropEquippedItem();
                }

                EquippedItemObject = _interactor.FocusedObject;
                _itemInteraction!.onEquipped();
            }
            else if (_equippedItem != null) 
            {
                DropEquippedItem();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (_itemInteraction != null) 
            {
                _itemInteraction.Attack();
            }
        }
    }

}