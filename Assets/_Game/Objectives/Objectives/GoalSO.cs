using UnityEngine;

[CreateAssetMenu(fileName = "NewGoal", menuName = "Game/Objectives")]
public class GoalSO : ScriptableObject
{
    public string Name;

    [TextArea]
    public string Description;
}
