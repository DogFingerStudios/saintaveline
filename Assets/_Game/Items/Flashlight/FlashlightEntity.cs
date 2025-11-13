using UnityEngine;

[System.Serializable]
public class FlashlightEntity : ItemEntity
{
    [SerializeField] public Light FlashlightLight;

    private AudioSource _click;

    private void Awake()
    {
        _click = this.GetComponent<AudioSource>();
        if (_click == null)
        {
            throw new System.Exception("FlashlightEntity: AudioSource component is missing.");
        }
    }

    [ItemAction("take_equip")]
    protected override void onTakeEquip()
    {
        Debug.Log("Flashlight equipped");
    }

    public override void PrimaryAction()
    {
        _click.Play();
        FlashlightLight.enabled = !FlashlightLight.enabled;
    }
}
