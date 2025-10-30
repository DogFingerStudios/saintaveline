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
    public Mission CreateMissionFromSO(MissionSO objectiveSO, MissionConfig config)
    {
        Mission objective = new(objectiveSO.Copy(), config);

        foreach (GoalSO goalSO in objectiveSO.Goals)
        {
            Goal goal = goalSO switch
            {
                ArriveAtGoalSO arriveAtGoalSO
                    => new ArriveAtGoal(arriveAtGoalSO.Copy()) { Host = config.Host },

                CollectItemGoalSO collectItemGoalSO
                    => new CollectItemGoal(collectItemGoalSO.Copy()) { Host = config.Host },

                _ => throw new Exception("Unknown GoalSO type.")
            };

            objective.Goals.Add(goal);
        }

        return objective;
    }
}