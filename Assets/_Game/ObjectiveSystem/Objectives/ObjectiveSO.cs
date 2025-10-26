using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "Game/Objective")]
public class  ObjectiveSO : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public List<GoalSO> Goals;
}
