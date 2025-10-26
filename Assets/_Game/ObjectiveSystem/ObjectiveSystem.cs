#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class Objective
{
    readonly ObjectiveSO         Data;

    public string       Name => Data.Name;
    public string       Description => Data.Description;

    public Stack<Goal>  Goals = new();
    public Goal?        CurrentGoal;

    public event Action OnObjectiveCompleted = null!;

    public Objective(ObjectiveSO obj)
    {
        Data = obj;
    }

    public void ManualAwake()
    {
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals.Peek();
            CurrentGoal.OnCompleted += GoalCompletedHandler;
        }
        else
        {
            throw new Exception("Objective must have at least one goal.");
        }
    }

    void GoalCompletedHandler()
    {
        if (CurrentGoal == null)
        {
            throw new Exception("CurrentGoal is null in GoalCompletedHandler.");
        }

        string msg = $"Completed goal '{CurrentGoal?.Name}'";
        Debug.Log(msg);
        BottomTypewriter.Instance.Enqueue(msg);

        Goals.Pop();
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals.Peek();
            CurrentGoal.OnCompleted += GoalCompletedHandler;
        }
        else
        {
            OnObjectiveCompleted?.Invoke();
        }
    }

    public void ManualUpdate()
    {
        if (CurrentGoal == null)
        {
            throw new Exception("CurrentGoal is null in Objective Update.");
        }
        
        CurrentGoal.ManualUpdate();
    }
}

public class ObjectiveSystem
{
    private static readonly Lazy<ObjectiveSystem> _instance =
        new (() => new ObjectiveSystem());

    public static ObjectiveSystem Instance => _instance.Value;

    public Objective? CurrentObjective;

    void ObjectiveCompleteHandler()
    {
        string msg = $"Completed objective '{CurrentObjective?.Name}'";
        Debug.Log(msg);
        BottomTypewriter.Instance.Enqueue(msg);
        CurrentObjective = null;
    }

    public void ManualAwake()
    {
        CurrentObjective!.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective?.ManualAwake();
    }

    public void ManualUpdate()
    {
        CurrentObjective?.ManualUpdate();
    }
}

public class ObjectiveFactory
{
    private static readonly Lazy<ObjectiveFactory> _instance =
        new (() => new ObjectiveFactory());

    public static ObjectiveFactory Instance => _instance.Value;

    public Goal CreateGoalFromSO(ArriveAtGoalSO arrivalGoalSO, CharacterEntity host)
    {
        return new ArriveAtGoal(arrivalGoalSO) { Host = host };
    }

    public Goal CreateGoalFromSO(CollectItemGoalSO collectItemSO, CharacterEntity host)
    {
        return new CollectItemGoal(collectItemSO) { Host = host };
    }

    // objectiveSO - the scriptable object defining the objective
    // host - the character entity that will be undertaking the objective
    public Objective CreateObjectiveFromSO(ObjectiveSO objectiveSO, CharacterEntity host)
    {
        Objective objective = new(objectiveSO);

        foreach (GoalSO goalSO in objectiveSO.Goals)
        {
            Goal goal = goalSO switch
            {
                ArriveAtGoalSO arriveAtGoalSO 
                    => new ArriveAtGoal(arriveAtGoalSO) { Host = host },

                CollectItemGoalSO collectItemGoalSO 
                    => new CollectItemGoal(collectItemGoalSO) { Host = host },

                _ => throw new Exception("Unknown GoalSO type.")
            };

            objective.Goals.Push(goal);
        }

        return objective;
    }
}