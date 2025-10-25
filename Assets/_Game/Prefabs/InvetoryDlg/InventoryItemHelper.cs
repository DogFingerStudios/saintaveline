#nullable enable

using UnityEngine;
using Image = UnityEngine.UI.Image;

public class InventoryItemHelper : MonoBehaviour
{
    public Image? Thumbnail = null;

    private ItemEntity? _itemEntity = null;
    public ItemEntity? ItemEntity { get => _itemEntity; set => _itemEntity = value; }
}
