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

