using UnityEngine;

public class FlashlightInteraction : ItemInteraction
{
    [InteractionAction("take_equip")]
    protected override void onTakeEquip()
    {
        Debug.Log("Flashlight equipped");
    }
}
