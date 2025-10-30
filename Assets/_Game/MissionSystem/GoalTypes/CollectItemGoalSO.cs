using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectItemGoal", menuName = "Game/Goals/CollectItemGoal")]
public class CollectItemGoalSO : GoalSO
{
    public string ItemName;
    public int QuantityNeeded = 1;
}
