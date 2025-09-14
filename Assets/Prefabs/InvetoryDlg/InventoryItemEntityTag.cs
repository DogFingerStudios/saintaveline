using UnityEngine;
using Toggle = UnityEngine.UI.Toggle;

public class InventoryItemEntityTag : MonoBehaviour
{
    [SerializeField] Toggle _toggle = null;
    public Toggle Toggle => _toggle;

    private ItemEntity? _itemEntity = null;
    public ItemEntity? ItemEntity { get; set; }
    

}
