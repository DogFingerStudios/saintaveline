#nullable enable
using System;
using System.Collections.Generic;

public abstract class GoalHandlerBase
{
    public List<Goal> Goals = new();
    
    public event Action<Goal>? OnGoalStarted;
    public event Action<Goal>? OnGoalCompleted;

    public event Action OnAllGoalsCompleted = null!;

    public void NotifyGoalStarted(Goal goal)
    {
        OnGoalStarted?.Invoke(goal);
    }

    public void NotifyGoalCompleted(Goal goal)
    {
        OnGoalCompleted?.Invoke(goal);
    }

    public void NotifyAllGoalsCompleted()
    {
        OnAllGoalsCompleted?.Invoke();
    }

    public abstract void StartMission();
    public abstract void ManualUpdate();
}

public class GoalHandlerSerial : GoalHandlerBase
{
    int _currentGoalIndex = 0;
    Goal? _currentGoal = null;

    public override void StartMission()
    {
        if (Goals.Count == 0)
        {
            throw new System.Exception("Mission must have at least one goal.");
        }

        _currentGoal = Goals[_currentGoalIndex];
        _currentGoal.OnGoalStarted += base.NotifyGoalStarted;
        _currentGoal.OnGoalCompleted += GoalCompletedHandler;
        _currentGoal.Start();
    }

    // this gets invoked from the concrete Goal implementation which 
    // determines when the goal is completed
    void GoalCompletedHandler(Goal goal)
    {
        if (goal != Goals[_currentGoalIndex])
        {
            throw new System.Exception("Completed goal does not match the current goal.");
        }

        NotifyGoalCompleted(goal);

        _currentGoalIndex++;
        if (_currentGoalIndex < Goals.Count)
        {
            _currentGoal = Goals[_currentGoalIndex];
            _currentGoal.OnGoalStarted += base.NotifyGoalStarted;
            _currentGoal.OnGoalCompleted += GoalCompletedHandler;
            _currentGoal.Start();
        }
        else
        {
            NotifyAllGoalsCompleted();
        }
    }

    public override void ManualUpdate()
    {
        _currentGoal!.ManualUpdate();
    }
}

public class GoalHandlerAsync : GoalHandlerBase
{
    public override void StartMission()
    {
        // Initialization logic for async goal handling
    }

    public override void ManualUpdate()
    {
        // Update logic for async goal handling
    }
}
