using System;
using System.Collections.Generic;
using UnityEngine;

public class Objective
{
    readonly ObjectiveSO Data;

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

    public List<Goal> Goals = new();
    public Goal? CurrentGoal;

    public event Action OnObjectiveCompleted = null!;

    public Objective(ObjectiveSO obj)
    {
        Data = obj;
    }

    public void ManualAwake()
    {
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals[0];
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

        Goals.RemoveAt(0);
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals[0];
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
