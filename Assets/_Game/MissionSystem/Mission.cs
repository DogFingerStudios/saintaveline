#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    public readonly MissionSO Data;
    public readonly MissionConfig RuntimeConfig;

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;    

    public List<Goal> Goals = new();

    public event Action OnMissionCompleted = null!;
    readonly GoalHandlerBase _goalHandler = null!;

    public Mission(MissionSO obj, MissionConfig runtimeConfig)
    {
        Data = obj;
        RuntimeConfig = runtimeConfig;

        if (Data.ConcurrentGoals)
        {
            _goalHandler = new GoalHandlerAsync() { Goals = Goals };
        }
        else
        {
            _goalHandler = new GoalHandlerSerial() { Goals = Goals };
        }

        _goalHandler.OnGoalStarted += GoalStartedHandler;
        _goalHandler.OnGoalCompleted += GoalCompletedHandler;
        _goalHandler.OnAllGoalsCompleted += AllGoalsCompletedHandler;
    }

    public void StartMission()
    {
        if (!StartMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(StartMessage);
        }
        
        _goalHandler.StartMission();
    }

    void GoalStartedHandler(Goal goal)
    {
        var goalIconObject = goal.MinimapIconObject;
        if (goalIconObject == null) return;

        if (goalIconObject.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;

        goalIconObject.GetComponent<GoalIconController>()
            .SetData(RuntimeConfig.MinimapCamera, RuntimeConfig.MinimapParent);
    }

    void GoalCompletedHandler(Goal goal)
    {
        goal.MinimapIconObject?.SetActive(false);
    }

    void AllGoalsCompletedHandler()
    {
        if (!SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(SuccessMessage);
        }
    }

    public void ManualUpdate()
    {
        _goalHandler.ManualUpdate();
    }
}
