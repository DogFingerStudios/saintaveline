using UnityEngine;

public class KnifeItemInteraction : ItemInteraction
{
     public KnifeItemInteraction()
     {
        // Default constructor
        Debug.Log("Knife item interaction default constructor called");
     }


    public override void Attack()
    {
        Debug.Log("PLayer attacked with knife");
    }
}
