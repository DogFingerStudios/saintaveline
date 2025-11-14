#nullable enable

using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;


// This class is attached to all characters (players, NPCs, etc.). For the player
// character, the class `PlayerStats` inherits from this class, and is attached
// to the root player GameObject.
// For NPCs, the class BaseNPC inherits from this class.
public class CharacterEntity : GameEntity
{
    [SerializeField] public Transform EquippedItemPos;
    private List<ItemEntity> _inventory = new List<ItemEntity>();
    public IReadOnlyList<ItemEntity> Inventory => _inventory.AsReadOnly();

    [SerializeField] private List<GameObject> _initialInventory = new List<GameObject>();
    [SerializeField] private GameObject? _initialEquippedItem = null;

    public UInt16 MaxInventorySize = 10;

    private ItemEntity? _equippedItem = null;
    public ItemEntity? EquippedItem { get => _equippedItem; }

    public virtual void Awake()
    {
        foreach (var itemObj in _initialInventory)
        {
            GameObject newItem = Instantiate(itemObj);
            var item = newItem.GetComponent<ItemEntity>();
            item.Initialize();
            this.AddItemToInventory(item);
        }

        if (_initialEquippedItem != null)
        {
            GameObject newItem = Instantiate(_initialEquippedItem);
            var item = newItem.GetComponent<ItemEntity>();
            item.Initialize();
            this.SetEquippedItem(item);
        }
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

    public void AddItemToInventory(ItemEntity item)
    {
        if (_inventory.Contains(item)) return;
        if (_inventory.Count >= MaxInventorySize)
        {
            BottomTypewriter.Instance.Enqueue("Inventory is full!");
            return;
        }

        if (item == _equippedItem)
        {
            item.OnUnEquipped();
            _equippedItem = null;
        }

        item.OnRemovePhysics();
        item.OnPickedUp(EquippedItemPos);
        item.gameObject.SetActive(false);
        _inventory.Add(item);
    }

    // used when transferring items between characters
    public void RemoveItemFromInventory(ItemEntity item)
    {
        _inventory.Remove(item);
    }

    public ItemEntity? SetEquippedItem(ItemEntity item, bool autoUnequip = false)
    {
        if (item.ItemData == null)
        {
            throw new System.Exception($"EquippedItem: Item '{item.name}' does not have ItemData.");
        }

        if (_equippedItem != null)
        {
            if (autoUnequip)
            {
                _equippedItem.OnUnEquipped();
                this.AddItemToInventory(_equippedItem);
            }
            else
            {
                throw new System.Exception($"EquippedItem: Character '{name}' already has an equipped item '{_equippedItem.name}'.");
            }
        }

        _equippedItem = item;
        _equippedItem.OnRemovePhysics();
        _equippedItem.OnPickedUp(EquippedItemPos);
        _equippedItem.OnEquipped();

        _inventory.Remove(_equippedItem);
        _equippedItem.gameObject.SetActive(true);

        return _equippedItem;
    }

    public void DropItem(ItemEntity item)
    {
        item.OnDropped();
        item.OnRestorePhysics();
        item.gameObject.SetActive(true);
        _inventory.Remove(item);
    }

    public ItemEntity? DropEquippedItem()
    {
        if (_equippedItem == null) return null;
        _equippedItem!.OnUnEquipped();
        this.DropItem(_equippedItem!);
        
        var droppedItem = _equippedItem;
        _equippedItem = null;
        return droppedItem;
    }

    public void ThrowEquippedItem()
    {
        if (_equippedItem == null) return;
        _equippedItem!.OnUnEquipped();
        _equippedItem!.OnDropped();
        _equippedItem!.OnRestorePhysics();

        if (_equippedItem.TryGetComponent<Rigidbody>(out var rb))
        {
            float _throwForce = 10f;
            rb.AddForce(Camera.main.transform.forward * _throwForce, ForceMode.VelocityChange);
        }

        _equippedItem = null;
    }

    public void Attack()
    {
        if (_equippedItem != null)
        {
            _equippedItem.Attack();
        }
    }

    public void PrimaryAction()
    {
        if (_equippedItem != null)
        {
            _equippedItem.PrimaryAction();
        }
    }

    public void EquipItem(int slot)
    {
        if (slot < 0)
        {
            throw new System.Exception($"EquipItem: Slot {slot} is out of range for character '{name}' inventory.");
        }

        if (slot >= _inventory.Count)
        {
            // slow it empty. ignore
            return;
        }

        var item = _inventory[slot];
        SetEquippedItem(item, autoUnequip: true);
    }
}