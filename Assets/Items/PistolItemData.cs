using UnityEngine;

[CreateAssetMenu(fileName = "NewPistolItem", menuName = "Game/PistolItem")]
public class PistolItemData : ItemData
{
    public float RecoilDuration;
    public float HoldDuration;
    public float ReturnDuration;
    
    public float FireRange;
    public Vector3 FirePoint;

    public AudioSource AudioSourcePrefab;
    public AudioClip FireSound;
}
