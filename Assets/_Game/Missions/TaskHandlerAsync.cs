using System.Collections.Generic;

public class TaskHandlerAsync : TaskHandlerBase
{
    List<Task> _inProcessTasks = new();

    public override void StartMission()
    {
        foreach (var task in Tasks)
        {
            _inProcessTasks.Add(task);
            task.OnTaskStarted += base.NotifyTaskStarted;
            task.OnTaskCompleted += GoalCompletedHandler;
            task.Start();
        }
    }

    void GoalCompletedHandler(Task task)
    {
        NotifyTaskCompleted(task);
        _inProcessTasks.Remove(task);
    }

    public override void ManualUpdate()
    {
        for (int i = _inProcessTasks.Count - 1; i >= 0; i--)
        {
            var task = _inProcessTasks[i];
            task.ManualUpdate();
        }

        if (_inProcessTasks.Count == 0)
        {
            NotifyAllTasksCompleted();
        }
    }
}
