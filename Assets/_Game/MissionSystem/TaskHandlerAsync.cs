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

        if (_inProcessTasks.Count == 0)
        {
            NotifyAllTasksCompleted();
        }
    }

    public override void ManualUpdate()
    {
        foreach (var task in _inProcessTasks)
        {
            task.ManualUpdate();
        }
    }
}
