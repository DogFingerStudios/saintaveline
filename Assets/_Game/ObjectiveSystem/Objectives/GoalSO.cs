using UnityEngine;

public enum GoalType
{
    ArriveAtLocation,
    CollectItem,
    //DefeatEnemy,
    //InteractWithObject
}

[CreateAssetMenu(fileName = "NewGoal", menuName = "Game/Goals/Goal")]
public class GoalSO : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public GoalType Type;
}

[CreateAssetMenu(fileName = "NewArriveAtGoal", menuName = "Game/Goals/ArriveAtGoal")]
public class ArriveAtGoalSO : GoalSO
{
    public Vector3 Location;
    public float   ArrivedDistance = 2.0f;
}

[CreateAssetMenu(fileName = "NewCollectItemGoal", menuName = "Game/Goals/CollectItemGoal")]
public class CollectItemGoalSO : GoalSO
{
    public string ItemName;
    public int    QuantityNeeded = 1;
}