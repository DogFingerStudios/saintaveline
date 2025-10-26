#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class Objective
{
    readonly ObjectiveSO         Data;

    public string       Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

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

        if (!CurrentGoal.SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(CurrentGoal.SuccessMessage);
        }

        string msg = $"Goal '{CurrentGoal.Name}' completed";
        Debug.Log(msg);

        Goals.Pop();
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals.Peek();
            CurrentGoal.OnCompleted += GoalCompletedHandler;

            if (!CurrentGoal.StartMessage.Equals(string.Empty))
            {
                BottomTypewriter.Instance.Enqueue(CurrentGoal.StartMessage);
            }
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
        if (CurrentObjective == null)
        {
            throw new Exception("CurrentObjective is null in ObjectiveCompleteHandler.");
        }

        string msg = $"Completed objective '{CurrentObjective?.Name}'";
        Debug.Log(msg);
        BottomTypewriter.Instance.Enqueue(msg);
        CurrentObjective = null;
    }

    public void ManualAwake()
    {
        if (CurrentObjective == null) return;

        CurrentObjective.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective.ManualAwake();

        if (!CurrentObjective.StartMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(CurrentObjective.StartMessage);
        }
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