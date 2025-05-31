using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public float DamageScore;
    
    public GameObject prefab; 
    
    public LayerMask TargetCollisionLayers; 

    public List<InteractionData> Interactions;
}

