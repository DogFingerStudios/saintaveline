using System;
using UnityEngine;

public struct ObjectiveConfig
{
    public CharacterEntity Host;
    public Camera MinimapCamera;
    public RectTransform MinimapParent;
    
}

public class ObjectiveFactory
{
    private static readonly Lazy<ObjectiveFactory> _instance =
        new(() => new ObjectiveFactory());

    public static ObjectiveFactory Instance => _instance.Value;

    // objectiveSO - the scriptable object defining the objective
    // host - the character entity that will be undertaking the objective
    public Objective CreateObjectiveFromSO(ObjectiveSO objectiveSO, ObjectiveConfig config)
    {
        Objective objective = new(objectiveSO.Copy(), config);

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