#nullable enable

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Assert = UnityEngine.Assertions.Assert;

// This class is attached to all characters (players, NPCs, etc.). For the player
// character, the class `PlayerStats` inherits from this class, and is attached
// to the root player GameObject.
// For NPCs, the class BaseNPC inherits from this class.
public class CharacterEntity : GameEntity
{
    public const UInt16 MaxInventorySize = 10;

    [SerializeField] public Transform EquippedItemPos;
    private readonly List<ItemEntity?> _inventory = Enumerable.Repeat<ItemEntity?>(null, MaxInventorySize).ToList();
    public IReadOnlyList<ItemEntity?> Inventory => _inventory.AsReadOnly();

    [SerializeField] private List<GameObject> _initialInventory = new List<GameObject>();
    [SerializeField] private GameObject? _initialEquippedItem = null;
    
    //private ItemEntity? _equippedItem = null;
    //public ItemEntity? EquippedItem { get => _equippedItem; }

    [SerializeField] private int? _equippedItemIndex = null; // the item the player is carrying
    public ItemEntity? EquippedItem2
    {
        get
        {
            if (_equippedItemIndex == null) return null;
            if (_equippedItemIndex < 0 || _equippedItemIndex >= _inventory.Count) return null;
            return _inventory[(int)_equippedItemIndex];
        }
    }

    public virtual void Awake()
    {
        foreach (var itemObj in _initialInventory)
        {
            GameObject newItem = Instantiate(itemObj);
            var item = newItem.GetComponent<ItemEntity>();
            item.Initialize();
            this.AddItemToInventory(item);
        }

        if (_equippedItemIndex != null)
        {
            var item = _inventory[(int)_equippedItemIndex];
            Assert.IsNotNull(item, 
                $"CharacterEntity.Awake: Initial equipped item index points to null slot");

            this.SetEquippedItem(item!);
        }
    }

    private int FirstFreeSlot()
    {
        for (int idx = 0; idx < _inventory.Count; idx++)
        {
            if (_inventory[idx] == null) return idx;
        }

        return -1;
    }

    public int AddItemToInventory(ItemEntity item)
    {
        if (_inventory.Contains(item)) return _inventory.IndexOf(item);

        var freeSlot = FirstFreeSlot();
        if (freeSlot == -1) return -1;

        item.OnRemovePhysics();
        item.OnPickedUp(EquippedItemPos); // set the item's position to the character's equipped item pos
        item.gameObject.SetActive(false);
        _inventory[freeSlot] = item;

        return freeSlot;
    }

    // used when transferring items between characters
    public void RemoveItemFromInventory(ItemEntity item)
    {
        int slot = _inventory.IndexOf(item);
        if (slot == -1)
        {
            throw new Exception($"RemoveItemFromInventory: Cannot remove item '{item.name}' from inventory, item not found.");
        }

        _inventory[slot] = null;
    }

    public ItemEntity? SetEquippedItem(ItemEntity item, bool autoUnequip = false)
    {
        if (item.ItemData == null)
        {
            throw new System.Exception($"EquippedItem: Item '{item.name}' does not have ItemData.");
        }

        if (_equippedItemIndex != null)
        {
            Assert.IsTrue(_equippedItemIndex >= 0 && _equippedItemIndex < _inventory.Count,
                $"EquippedItem: _equippedItemIndex {_equippedItemIndex} is out of range for character '{name}' inventory.");

            var equippedItem = _inventory[_equippedItemIndex.Value];
            Assert.IsNotNull(equippedItem, $"SetEquippedItem: Equipped item set to slot '{_equippedItemIndex.Value}', but slot is empty");

            if (autoUnequip)
            {
                equippedItem!.OnRemovePhysics();
                equippedItem!.OnPickedUp(EquippedItemPos); // set the item's position to the character's equipped item pos
                equippedItem!.gameObject.SetActive(false);
                equippedItem!.OnUnEquipped(); 
            }
            else
            {
                throw new System.Exception($"EquippedItem: Character '{name}' already has an equipped item '{equippedItem!.name}'.");
            }
        }

        var itemIndex = _inventory.IndexOf(item);
        if (itemIndex == -1) itemIndex = AddItemToInventory(item);

        // can happen if all slots in inventory are full
        if (itemIndex == -1) return null;

        Assert.IsTrue(itemIndex >= 0 && itemIndex < _inventory.Count && itemIndex < MaxInventorySize,
            $"EquippedItem: itemIndex {itemIndex} is out of range for character '{name}' inventory.");   

        item.OnRemovePhysics();
        item.OnPickedUp(EquippedItemPos); // set the item's position to the character's equipped item pos
        item.gameObject.SetActive(true);
        item.OnEquipped();
        
        _equippedItemIndex = itemIndex; 

        return item;
    }

    // can be called on a non-equipped item
    public void DropItem(ItemEntity item)
    {
        int slotIdx = _inventory.IndexOf(item);
        if (slotIdx == -1)
        {
            throw new Exception($"DropItem: Item '{item.name}' cannot be dropped. Item is not in inventory.");
        }

        item.OnDropped();
        item.OnRestorePhysics();
        item.gameObject.SetActive(true);
        
        _inventory[slotIdx] = null;
    }

    public ItemEntity? DropEquippedItem()
    {
        if (_equippedItemIndex == null) return null;
        Assert.IsNotNull(_inventory[_equippedItemIndex.Value],
            $"DropEquippedItem: Equipped Item Index is null");

        var item = _inventory[_equippedItemIndex.Value];
        item!.OnUnEquipped();
        this.DropItem(item!);

        _equippedItemIndex = null;
        return item;
    }

    public void ThrowEquippedItem()
    {
        if (_equippedItemIndex == null) return;

        Assert.IsNotNull(_inventory[_equippedItemIndex.Value],
            $"ThrowEquippedItem: Equipped item's inventory slot is null");

        var item = _inventory[_equippedItemIndex.Value];
        item!.OnUnEquipped();
        item!.OnDropped();
        item!.OnRestorePhysics();

        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            float _throwForce = 10f;
            rb.AddForce(Camera.main.transform.forward * _throwForce, ForceMode.VelocityChange);
        }

        _inventory[(int)_equippedItemIndex] = null;
        _equippedItemIndex = null;
    }

    public void Attack()
    {
        if (_equippedItemIndex == null) return;

        Assert.IsNotNull(_inventory[_equippedItemIndex.Value],
            $"Attack: Equipped item's inventory slot is null");

        _inventory[(int)_equippedItemIndex]!.Attack();
    }

    public void PrimaryAction()
    {
        if (_equippedItemIndex == null) return;

        Assert.IsNotNull(_inventory[_equippedItemIndex.Value],
            $"PrimaryAction: Equipped item's inventory slot is null");

        _inventory[(int)_equippedItemIndex]!.PrimaryAction();  
    }

    // will equip the item in the specified slot, 
    // or unequip if already equipped
    public void ToggleEquippedItem(int slot)
    {
        if (slot < 0 || slot >= MaxInventorySize)
        {
            throw new System.Exception($"ToggleEquippedItem: Slot {slot} is out of range for character '{name}' inventory.");
        }

        if (_equippedItemIndex != null && _equippedItemIndex == slot)
        {
            
            var equippedItem = _inventory[slot];
            Assert.IsNotNull(equippedItem,
                $"ToggleEquippedItem: Equipped item at slot {slot} is null for character '{name}'.");

            // TODO: this unequip logic is duplicated in a few places,
            // should refactor into its own method
            equippedItem!.OnRemovePhysics();
            equippedItem!.OnPickedUp(EquippedItemPos); // set the item's position to the character's equipped item pos
            equippedItem!.gameObject.SetActive(false);
            equippedItem!.OnUnEquipped(); 

            _equippedItemIndex = null;
            return;
        }

        var item = _inventory[slot];
        if (item == null) return;

        SetEquippedItem(item!, autoUnequip: true);
    }

    public override float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        RaiseOnHealthChanged(Health);
        return Health;
    }

    public override float TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        RaiseOnHealthChanged(Health);
        return Health;
    }
}