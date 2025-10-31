#nullable enable
using System;
using System.Collections.Generic;

public abstract class TaskHandlerBase
{
    public List<Task> Tasks = new();
    
    public event Action<Task>? OnTaskStarted;
    public event Action<Task>? OnTaskCompleted;

    public event Action OnAllTasksCompleted = null!;

    public void NotifyTaskStarted(Task task)
    {
        OnTaskStarted?.Invoke(task);
    }

    public void NotifyTaskCompleted(Task task)
    {
        OnTaskCompleted?.Invoke(task);
    }

    public void NotifyAllTasksCompleted()
    {
        OnAllTasksCompleted?.Invoke();
    }

    public abstract void StartMission();
    public abstract void ManualUpdate();
}

