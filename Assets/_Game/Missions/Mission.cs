#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    public readonly MissionSO Data;
    public readonly MissionConfig RuntimeConfig;

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;    

    public List<Task> Tasks = new();

    public event Action OnMissionCompleted = null!;
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
        if (!StartMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(StartMessage);
        }
        
        _taskHandler.StartMission();
    }

    void TaskStartedHandler(Task task)
    {
        var taskIconObject = task.MinimapIconObject;
        if (taskIconObject == null) return;

        if (taskIconObject.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;

        taskIconObject.GetComponent<TaskIconController>()
            .SetData(RuntimeConfig.MinimapCamera, RuntimeConfig.MinimapParent);
    }

    void TaskCompletedHandler(Task task)
    {
        task.MinimapIconObject?.SetActive(false);
    }

    void AllTasksCompletedHandler()
    {
        if (!SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(SuccessMessage);
        }

        OnMissionCompleted?.Invoke();
    }

    public void ManualUpdate()
    {
        _taskHandler.ManualUpdate();
    }
}
