using UnityEngine;

// This script is typically attached to an item in the game world to 
// allow the player to interact with it.
public class ItemPickup : MonoBehaviour, Interactable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private ItemInteractMenu _menu;

    public string HelpText => $"Press [E] to interact with '{_itemData.ItemName}'";

    public void Interact()
    {
        InteractionManager.Instance.OpenMenu(_itemData.Interactions);
    }

    public void OnDefocus()
    {
        // nothing to do
    }

    public void OnFocus()
    {
        // nothing to do
    }

    void Start()
    {
        // nothing to do
    }

    void Update()
    {
        // nothing to do
    }
}
