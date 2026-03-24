using System.Collections.Generic;

// NOTE: The "aync" in this class name is a lie. This is not a traditional type
// of asynchronicity, this is more about having multiple tasks in progress at
// the same time
public class TaskHandlerAsync : TaskHandlerBase
{
    List<Task> _inProcessTasks = new();
    bool _succeeded = true;

    public override void StartMission()
    {
        foreach (var task in Tasks)
        {
            _inProcessTasks.Add(task);
            task.OnTaskStarted += base.NotifyTaskStarted;
            task.OnTaskCompleted += TaskCompletedHandler;
            task.Start();
        }
    }

    void TaskCompletedHandler(Task task)
    {
        NotifyTaskCompleted(task);
        _inProcessTasks.Remove(task);
        
        if (task.State == TaskState.Failed)
        {
            _succeeded = false;
            _inProcessTasks.Clear();
        }
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
            NotifyAllTasksCompleted(_succeeded);
        }
    }
}
