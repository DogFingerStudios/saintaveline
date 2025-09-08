using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    private List<ItemEntity> _items = new List<ItemEntity>();
    public List<ItemEntity> Items { get => _items; }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
