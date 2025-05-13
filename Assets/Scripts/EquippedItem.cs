#nullable enable

using UnityEngine;

// this script is attached to the player character and defines where
// items the player is holding should be placed in the world
public class EquippedItem : MonoBehaviour
{
    [SerializeField] private Transform _equippedItemPos;
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
        }
    }  

    public void DropEquippedItem() 
    {
        if (_equippedItem != null) 
        {
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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (_equippedItem != null) 
            {
                DropEquippedItem();
            }
        }
    }

}