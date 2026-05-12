using UnityEngine;

// AI: Spawns item(s) at NPC location or gives to player
[CreateAssetMenu(fileName = "GiveItemAction", menuName = "Game/Dialogue Actions/Give Item")]
public class GiveItemAction : DialogueActionSO
{
    [SerializeField] private string _itemId;
    [SerializeField] private int _quantity = 1;
    [SerializeField] private bool _dropAtNPCLocation = false;

    public override void Execute(DialogueActionContext context)
    {
        if (string.IsNullOrEmpty(_itemId))
        {
            Debug.LogWarning("GiveItemAction: Item ID is empty!");
            return;
        }

        if (_dropAtNPCLocation && context.NPC != null)
        {
            Debug.Log($"NPC {context.NPC.name} drops {_quantity}x {_itemId} at their location");
            // AI: Instantiate item prefab at NPC position
            // Example: Instantiate(itemPrefab, context.NPC.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log($"Player receives {_quantity}x {_itemId}");
            // AI: Add to player inventory system
        }
    }
}
