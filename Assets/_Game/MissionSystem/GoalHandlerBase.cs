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

public class GoalHandlerAsync : GoalHandlerBase
{
    List<Goal> _inProcessGoals = new();

    public override void StartMission()
    {
        foreach (var goal in Goals)
        {
            _inProcessGoals.Add(goal);
            goal.OnGoalStarted += base.NotifyGoalStarted;
            goal.OnGoalCompleted += GoalCompletedHandler;
            goal.Start();
        }
    }

    void GoalCompletedHandler(Goal goal)
    {
        NotifyGoalCompleted(goal);

        _inProcessGoals.Remove(goal);

        if (_inProcessGoals.Count == 0)
        {
            NotifyAllGoalsCompleted();
        }
    }

    public override void ManualUpdate()
    {
        foreach (var goal in _inProcessGoals)
        {
            goal.ManualUpdate();
        }
    }
}
