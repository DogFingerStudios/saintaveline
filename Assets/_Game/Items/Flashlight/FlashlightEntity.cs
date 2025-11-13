using UnityEngine;

[System.Serializable]
public class FlashlightEntity : ItemEntity
{
    [SerializeField] public Light FlashlightLight;

    [ItemAction("take_equip")]
    protected override void onTakeEquip()
    {
        Debug.Log("Flashlight equipped");
    }

    public override void PrimaryAction()
    {
        FlashlightLight.enabled = !FlashlightLight.enabled;
    }
}
