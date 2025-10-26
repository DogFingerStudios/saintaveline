using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectItemGoal", menuName = "Game/Goals/CollectItemGoal")]
public class CollectItemGoalSO : GoalSO
{
    public string ItemName;
    public int QuantityNeeded = 1;
}

public class CollectItemGoal : Goal
{
    public string ItemName => Data.ItemName;
    public int    QuantityNeeded => Data.QuantityNeeded;

    private int   _quantityCollected = 0;

    private CollectItemGoalSO Data => this.TypedData<CollectItemGoalSO>();

    public CollectItemGoal(CollectItemGoalSO data)
        : base(data)
    {
        // nothing to do
    }
}
