using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGoal", menuName = "Game/Goals/Goal")]
public class GoalSO : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public string StartMessage;
    public string SuccessMessage;
    public string FailureMessage;
}

public class Goal
{
    private readonly GoalSO Data;
    public T TypedData<T>() => (T)(object)Data;

    // public setter allows for the player to assign the goal/task to someone else,
    // though we'll probably eventually want a setter function, as there may be
    // things to do when assigning a new host (like updating UI, etc).
    public CharacterEntity? Host { get; set; }

    public string Name => Data.Name;
    public string Description => Data.Description;
    public string StartMessage => Data.StartMessage;
    public string SuccessMessage => Data.SuccessMessage;
    public string FailureMessage => Data.FailureMessage;

    public event Action? OnCompleted;

    public Goal(GoalSO data)
    {
        Data = data;
    }

    public void Complete()
    {
        OnCompleted?.Invoke();
    }

    public virtual void ManualUpdate()
    {
        // nothing to do
    }
}
