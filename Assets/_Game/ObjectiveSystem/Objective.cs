#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class Objective
{
    readonly ObjectiveSO Data;
    readonly ObjectiveConfig RuntimeConfig;

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

    public List<Objective> Prerequisites = new();
    public Objective? Next;

    public List<Goal> Goals = new();
    public Goal? CurrentGoal;

    public event Action OnObjectiveCompleted = null!;

    public Objective(ObjectiveSO obj, ObjectiveConfig runtimeConfig)
    {
        Data = obj;
        RuntimeConfig = runtimeConfig;
    }

    public void StartObjective()
    {
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals[0];
            CurrentGoal.OnStarted += GoalStartedHandler;
            CurrentGoal.OnCompleted += GoalCompletedHandler;

            if (!StartMessage.Equals(string.Empty))
            {
                BottomTypewriter.Instance.Enqueue(StartMessage);
            }

            CurrentGoal.Start();
        }
        else
        {
            throw new Exception("Objective must have at least one goal.");
        }
    }

    void GoalStartedHandler()
    {
        if (CurrentGoal == null)
        {
            throw new Exception("CurrentGoal is null in GoalStartedHandler.");
        }

        var goalIconObject = CurrentGoal.MinimapIconObject;
        if (goalIconObject == null) return;

        if (goalIconObject.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;

        goalIconObject.GetComponent<GoalIconController>()
            .SetData(RuntimeConfig.MinimapCamera, RuntimeConfig.MinimapParent);
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
            CurrentGoal.OnStarted += GoalStartedHandler;
            CurrentGoal.OnCompleted += GoalCompletedHandler;
            CurrentGoal.Start();
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
