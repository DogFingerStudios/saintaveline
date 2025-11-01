using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionSO : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public string ShortDescription;

    public string StartMessage;
    public string SuccessMessage;
    public string FailureMessage;

    public bool ConcurrentTasks = false;
    public List<TaskSO> Tasks;
}
