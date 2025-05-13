#nullable enable
using UnityEngine;

// this script is attached to the player character and defines where
// items the player is holding should be placed in the world
public class EquippedItem : MonoBehaviour
{
    private GameObject? _equippedItemVar;
    public GameObject? EquippedItemVar
    {
        get => _equippedItemVar;
        set => _equippedItemVar = value;
    }

    [SerializeField] private Transform? _equippedItemPos = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // nothing to do
    }

    // Update is called once per frame
    void Update()
    {
        if (!_equippedItemVar || !_equippedItemPos) return;

        _equippedItemVar!.transform.position = _equippedItemPos.position;
        _equippedItemVar!.transform.rotation = _equippedItemPos.rotation;
    }
}
