using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public float DamageScore;

    public List<string> Interactions;

    // public Sprite icon;  // For UI display
    // public GameObject prefab; //  Link the 3D model here!
}

