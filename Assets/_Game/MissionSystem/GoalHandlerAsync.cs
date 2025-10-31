using System.Collections.Generic;

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
