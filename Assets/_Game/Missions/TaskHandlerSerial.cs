#nullable enable

public class TaskHandlerSerial : TaskHandlerBase
{
    int _currentTaskIndex = 0;
    Task? _currentTask = null;

    public override void StartMission()
    {
        if (Tasks.Count == 0)
        {
            throw new System.Exception("Mission must have at least one task.");
        }

        _currentTask = Tasks[_currentTaskIndex];
        _currentTask.OnTaskStarted += base.NotifyTaskStarted;
        _currentTask.OnTaskCompleted += TaskCompletedHandler;
        _currentTask.Start();
    }

    // this gets invoked from the concrete Task implementation which 
    // determines when the task is completed
    void TaskCompletedHandler(Task task)
    {
        if (task != Tasks[_currentTaskIndex])
        {
            throw new System.Exception("Completed task does not match the current task.");
        }

        NotifyTaskCompleted(task);

        _currentTaskIndex++;
        if (_currentTaskIndex < Tasks.Count)
        {
            _currentTask = Tasks[_currentTaskIndex];
            _currentTask.OnTaskStarted += base.NotifyTaskStarted;
            _currentTask.OnTaskCompleted += TaskCompletedHandler;
            _currentTask.Start();
        }
        else
        {
            NotifyAllTasksCompleted();
        }
    }

    public override void ManualUpdate()
    {
        _currentTask!.ManualUpdate();
    }
}