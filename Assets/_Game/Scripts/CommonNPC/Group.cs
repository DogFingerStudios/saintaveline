using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using System.Collections.Concurrent;

public class GroupAction
{
    public string ID;
    public Action Action;
    public float Cooldown;
    public BaseNPC Initiator;
}

[System.Serializable]
public class Group
{
    [SerializeField]
    public List<BaseNPC> Members = new();

    private ConcurrentDictionary<string, byte> _executedActions = new();

    private async void ScheduleRemove(string id, float coolDown)
    {         
        await System.Threading.Tasks.Task.Delay((int)(coolDown * 1000));
        _executedActions.TryRemove(id, out _);
    }

    public BaseNPC? TakeAction(GroupAction action)
    {
        if (_executedActions.ContainsKey(action.ID)) return null;

        action.Action();
        _executedActions.TryAdd(action.ID, 0);

        if (action.Cooldown > 0.0f) ScheduleRemove(action.ID, action.Cooldown);

        return action.Initiator;
    }
}
