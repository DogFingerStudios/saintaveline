using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class  MissionSO : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public List<GoalSO> Goals;
    
    public string StartMessage;
    public string SuccessMessage;
    public string FailureMessage; 
}
