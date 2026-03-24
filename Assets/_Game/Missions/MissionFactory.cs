using System;
using UnityEngine;

public struct MissionConfig
{
    public CharacterEntity Host;
    public Camera MinimapCamera;
    public RectTransform MinimapParent;
}

public class MissionFactory
{
    private static readonly Lazy<MissionFactory> _instance =
        new(() => new MissionFactory());

    public static MissionFactory Instance => _instance.Value;

    // objectiveSO - the scriptable object defining the objective
    // host - the character entity that will be undertaking the objective
    public Mission CreateMissionFromSO(MissionSO missionSO, MissionConfig config)
    {
        Mission objective = new(missionSO.Copy(), config);

        foreach (TaskSO taskSO in missionSO.Tasks)
        {
            Task task = taskSO switch
            {
                ArriveAtTaskSO arriveAtGoalSO
                    => new ArriveAtTask(arriveAtGoalSO.Copy()) { Host = config.Host },

                CollectItemGoalSO collectItemGoalSO
                    => new CollectItemGoal(collectItemGoalSO.Copy()) { Host = config.Host },

                TimedArriveAtTaskSO timedArriveAtTaskSO
                    => CreateTimedTask(missionSO, timedArriveAtTaskSO.Copy(), config.Host),

                _ => throw new Exception("Unknown GoalSO type.")
            };

            objective.Tasks.Add(task);
        }

        return objective;
    }

    private Task CreateTimedTask(MissionSO missionSO, TimedArriveAtTaskSO so, CharacterEntity host)
    {
        if (missionSO.ConcurrentTasks)
        {
            throw new Exception("Timed tasks are not supported for concurrent missions.");
        }

        return new TimedArriveAtTask(so.Copy()) { Host = host };
    }
}