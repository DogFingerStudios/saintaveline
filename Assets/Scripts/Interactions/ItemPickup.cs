using UnityEngine;

public class ItemPickup : MonoBehaviour, Interactable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private ItemInteractMenu _menu;

    // print the damageScore in the HelpText
    public string HelpText => $"Press [E] to interact with '{_itemData.ItemName}'";

    public void Interact()
    {
        InteractionManager.Instance.DoIt();
        InteractionManager.Instance.OpenMenu(_itemData.Interactions);
        // _menu.Open();
    }

    public void OnDefocus()
    {
        Debug.Log("Item is defocused!!");
    }

    public void OnFocus()
    {
        Debug.Log("Item is focused!!");
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
