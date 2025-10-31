using System.Collections.Generic;
using UnityEngine;

public abstract class GoalHandlerBase
{
    public List<Goal> Goals = new();
    
    public event System.Action OnAllGoalsCompleted = null!;

    public void NotifyAllGoalsCompleted()
    {
        OnAllGoalsCompleted?.Invoke();
    }

    public abstract void StartMission();
}

public class GoalHandlerSerial : GoalHandlerBase
{
    Goal? _currentGoal;

    public override void StartMission()
    {
        if (Goals.Count == 0)
        {
            throw new System.Exception("Mission must have at least one goal.");
        }

        _currentGoal = Goals[0];
        _currentGoal.OnStarted += GoalStartedHandler;
        _currentGoal.OnCompleted += GoalCompletedHandler;
        _currentGoal.Start();
    }

    void GoalStartedHandler()
    {
        // Handle goal started event
    }

    void GoalCompletedHandler()
    {
        // int currentIndex = Goals.IndexOf(_currentGoal!);
        // if (currentIndex + 1 < Goals.Count)
        // {
        //     _currentGoal = Goals[currentIndex + 1];
        //     _currentGoal.OnStarted += GoalStartedHandler;
        //     _currentGoal.OnCompleted += GoalCompletedHandler;

        //     _currentGoal.Start();
        // }
        // else
        // {
        //     NotifyAllGoalsCompleted();
        // }
    }
}

public class GoalHandlerAsync : GoalHandlerBase
{
    public override void StartMission()
    {
        // Initialization logic for async goal handling
    }
}
