#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public enum MissionState
{
    Inactive,   // mission has not yet started
    InProgress,
    Completed,
    Failed
}

public class Mission
{
    public readonly MissionSO Data;
    public readonly MissionConfig RuntimeConfig;
    public MissionState State { get; private set; } = MissionState.Inactive;

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

    public List<Task> Tasks = new();

    public event Action<Task> OnTaskStarted = null!;
    public event Action<Task> OnTaskCompleted = null!;
    public event Action<Task> OnTaskTick = null!;

    public event Action<Mission> OnMissionStarted = null!;
    public event Action<Mission> OnMissionCompleted = null!;

    readonly TaskHandlerBase _taskHandler = null!;

    public Mission(MissionSO obj, MissionConfig runtimeConfig)
    {
        Data = obj;
        RuntimeConfig = runtimeConfig;

        if (Data.ConcurrentTasks)
        {
            _taskHandler = new TaskHandlerAsync() { Tasks = Tasks };
        }
        else
        {
            _taskHandler = new TaskHandlerSerial() { Tasks = Tasks };
        }

        _taskHandler.OnTaskStarted += TaskStartedHandler;
        _taskHandler.OnTaskCompleted += TaskCompletedHandler;
        _taskHandler.OnAllTasksCompleted += AllTasksCompletedHandler;
    }

    public void StartMission()
    {
        this.State = MissionState.InProgress;
        OnMissionStarted?.Invoke(this);
        _taskHandler.StartMission();
    }

    void TaskStartedHandler(Task task)
    {
        var taskIconObject = task.MinimapIconObject;
        if (taskIconObject == null) return;

        if (taskIconObject.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;

        taskIconObject.GetComponent<TaskIconController>()
            .SetData(RuntimeConfig.MinimapCamera, RuntimeConfig.MinimapParent);

        task.OnTaskTick += (task) => OnTaskTick?.Invoke(task);

        OnTaskStarted?.Invoke(task);
    }

    void TaskCompletedHandler(Task task)
    {
        task.MinimapIconObject?.SetActive(false);
        OnTaskCompleted?.Invoke(task);
    }

    void AllTasksCompletedHandler(bool success)
    {
        this.State = success ? MissionState.Completed : MissionState.Failed;
        OnMissionCompleted?.Invoke(this);
    }

    public void ManualUpdate()
    {
        _taskHandler.ManualUpdate();
    }
}
