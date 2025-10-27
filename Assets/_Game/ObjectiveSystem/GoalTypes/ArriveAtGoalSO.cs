using UnityEngine;

[CreateAssetMenu(fileName = "NewArriveAtGoal", menuName = "Game/Goals/ArriveAtGoal")]
public class ArriveAtGoalSO : GoalSO
{
    public Vector3 Location;
    public float ArrivedDistance = 2.0f;
}
