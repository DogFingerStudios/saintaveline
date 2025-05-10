using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public float damageScore;

    public Sprite icon;  // For UI display
    public GameObject prefab; //  Link the 3D model here!

}

