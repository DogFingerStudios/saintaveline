using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "TimedArriveAtTask", menuName = "Game/Goals/TimedArriveAtTask")]
public class TimedArriveAtTaskSO : TaskSO
{
    public float ArrivedDistance = 2.0f;
    public float TimeLimit = 60.0f; // seconds
    public bool ShowTimer = true;
}
