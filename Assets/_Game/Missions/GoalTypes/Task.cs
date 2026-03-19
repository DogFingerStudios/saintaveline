#nullable enable
using System;
using UnityEngine;

public enum TaskState
{
    Inactive,   // task has not yet started
    InProgress,
    Completed,
    Failed
}

public class Task
{
    private readonly TaskSO Data;
    public T TypedData<T>() => (T)(object)Data;

    // public setter allows for the player to assign the task to someone else,
    // though we'll probably eventually want a setter function, as there may be
    // things to do when assigning a new host (like updating UI, etc).
    public CharacterEntity? Host { get; set; }

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

    public Action<Task>? OnTaskStarted;
    public event Action<Task>? OnTaskCompleted;
    public event Action<Task>? OnTaskTick;

    public GameObject? MinimapIconObject { get; private set; }

    public Task(TaskSO data) => this.Data = data;

    public TaskState State = TaskState.Inactive;

    protected void Complete(bool success = true)
    {
        MinimapIconObject?.SetActive(false);

        if (success)
        {
            if (!this.SuccessMessage.Equals(string.Empty))
            {
                BottomTypewriter.Instance.Enqueue(this.SuccessMessage);
            }

            State = TaskState.Completed;
        }
        else
        {
            if (!this.FailureMessage.Equals(string.Empty))
            {
                BottomTypewriter.Instance.Enqueue(this.FailureMessage);
            }

            State = TaskState.Failed;
        }

        OnTaskCompleted?.Invoke(this);
    }

    public virtual void Start()
    {
        if (Data.MinimapIcon != null)
        {
            MinimapIconObject = UnityEngine.Object.Instantiate(Data.MinimapIcon, Data.Location, Data.MinimapIcon.transform.rotation);
        }

        if (!this.StartMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(this.StartMessage);
        }

        State = TaskState.InProgress;
        OnTaskStarted?.Invoke(this);
    }

    public virtual void ManualUpdate()
    {
        OnTaskTick?.Invoke(this);
    }
}