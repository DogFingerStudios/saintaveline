using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using System.Collections.Concurrent;

public class PartnershipAction
{
    public string ID;
    public Action Action;
    public float Cooldown;
}

[System.Serializable]
public class Partnership
{
    [SerializeField]
    public List<BaseNPC> Partners = new();

    private HashSet<string> executedActions = new();
    private ConcurrentDictionary<string, byte> _executedActions2 = new();

    public bool TakeAction(PartnershipAction partnershipAction)
    {
        return TakeAction(partnershipAction.ID, partnershipAction.Action);
    }

    private async void ScheduleRemove(string id, float coolDown)
    {         
        await System.Threading.Tasks.Task.Delay((int)(coolDown * 1000));
        _executedActions2.TryRemove(id, out _);
    }

    // returns true if action was executed, false if it was already executed before
    public bool TakeAction(string id, Action action, float coolDown = 0.0f)
    {
        if (_executedActions2.ContainsKey(id)) return false;

        action();
        _executedActions2.TryAdd(id, 0);

        if (coolDown > 0.0f) ScheduleRemove(id, coolDown);
        
        return true;
    }
}
