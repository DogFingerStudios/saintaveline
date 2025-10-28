#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveSystem
{
    private static readonly Lazy<ObjectiveSystem> _instance =
        new (() => new ObjectiveSystem());

    public static ObjectiveSystem Instance => _instance.Value;

    public Objective? CurrentObjective;

    void ObjectiveCompleteHandler()
    {
        if (CurrentObjective == null)
        {
            throw new Exception("CurrentObjective is null in ObjectiveCompleteHandler.");
        }

        string msg = $"Completed objective '{CurrentObjective.Name}'";
        Debug.Log(msg);

        if (!CurrentObjective.SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(CurrentObjective.SuccessMessage);
        }

        CurrentObjective = null;
    }

    private RunOnce? _runonce;

    public void ManualAwake()
    {
        if (CurrentObjective == null) return;

        CurrentObjective.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective.ManualAwake();

        if (!CurrentObjective.StartMessage.Equals(string.Empty))
        {
            _runonce = new RunOnce()
            {
                PreCalls = 1,
                Func = () =>
                {
                    BottomTypewriter.Instance.Enqueue(CurrentObjective.StartMessage);

                    if (CurrentObjective.CurrentGoal != null &&
                        !CurrentObjective.CurrentGoal.StartMessage.Equals(string.Empty))
                    {
                        BottomTypewriter.Instance.Enqueue(CurrentObjective.CurrentGoal.StartMessage);
                    }
                }
            };
        }
    }

    public void ManualUpdate(MonoBehaviour mb)
    {
        _runonce?.Run();
        if (CurrentObjective == null) return;

        CurrentObjective.ManualUpdate();
    }
}
