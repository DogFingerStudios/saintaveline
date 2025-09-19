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
    [SerializeField] private Transform _equippedItemPos;

    private List<ItemEntity> _inventory = new List<ItemEntity>();
    public IReadOnlyList<ItemEntity> Inventory => _inventory.AsReadOnly();

    public UInt16 MaxInventorySize = 10;


    private ItemEntity? _equippedItem = null;
    public ItemEntity? EquippedItem { get => _equippedItem; }
    
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

        if (_inventory.Count >= _maxInventorySize)
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
        item.OnPickedUp(_equippedItemPos);
        item.gameObject.SetActive(false);
        _inventory.Add(item);
    }

    public ItemEntity? SetEquippedItem(ItemEntity item)
    {
        if (item.ItemData == null)
        {
            throw new System.Exception($"EquippedItem: Item '{item.name}' does not have ItemData.");
        }

        if (_equippedItem != null)
        {
            // TODO: auto store an equip item, and if the item is not 
            // storable, then show a message to the player.
            throw new System.NotImplementedException("EquippedItem: Unequipping an item when one is already equipped is not implemented.");
        }

        _equippedItem = item;
        _equippedItem.OnRemovePhysics();
        _equippedItem.OnPickedUp(_equippedItemPos);
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
}