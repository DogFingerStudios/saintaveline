#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Goal
{ 
    public string Name;
    public string Description;
    
    public event Action OnCompleted;
    public void Complete()
    {
        OnCompleted?.Invoke();
    }

    public virtual void ManualUpdate()
    {
        // nothing to do
    }
}

public class ArriveAtGoal : Goal
{
    public Vector3  Location;
    public float    ArrivedDistance;

    public Transform ChracterTransform;

    public ArriveAtGoal(Transform characterTransform)
    {
        ChracterTransform = characterTransform;
    }   

    public override void ManualUpdate()
    {
        if (Vector3.Distance(ChracterTransform.position, Location) <= ArrivedDistance)
        {
            base.Complete();
        }
    }
}

public class Objective
{
    public string       Name;
    public string       Description;
    public Stack<Goal>  Goals;
    public Goal?        CurrentGoal;

    public event Action OnObjectiveCompleted;

    public void ManualAwake()
    {
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals.Peek();
            CurrentGoal.OnCompleted += GoalCompletedHandler;
        }
        else
        {
            throw new Exception("Objective must have at least one goal.");
        }
    }

    void GoalCompletedHandler()
    {
        if (CurrentGoal == null)
        {
            throw new Exception("CurrentGoal is null in GoalCompletedHandler.");
        }

        Goals.Pop();
        if (Goals.Count > 0)
        {
            CurrentGoal = Goals.Peek();
            CurrentGoal.OnCompleted += GoalCompletedHandler;
        }
        else
        {
            OnObjectiveCompleted?.Invoke();
        }
    }

    public void ManualUpdate()
    {
        if (CurrentGoal == null)
        {
            throw new Exception("CurrentGoal is null in Objective Update.");
        }
        
        CurrentGoal.ManualUpdate();
    }
}

public class ObjectiveSystem
{
    private static readonly Lazy<ObjectiveSystem> _instance =
        new Lazy<ObjectiveSystem>(() => new ObjectiveSystem());

    public static ObjectiveSystem Instance => _instance.Value;

    public Objective? CurrentObjective;

    public void ManualAwake()
    {
        CurrentObjective?.ManualAwake();
    }

    public void ManualUpdate()
    {
        CurrentObjective?.ManualUpdate();
    }
}
