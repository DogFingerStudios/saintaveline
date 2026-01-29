using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using System.Collections.Concurrent;

//public class PartnershipAction
//{
//    public string ID;
//    public Action Action;
//    public float Cooldown;
//}

[System.Serializable]
public class Group
{
    [SerializeField]
    public List<BaseNPC> Members = new();

    private ConcurrentDictionary<string, byte> _executedActions = new();

    //public bool TakeAction(PartnershipAction partnershipAction)
    //{
    //    return TakeAction(partnershipAction.ID, partnershipAction.Action);
    //}

    private async void ScheduleRemove(string id, float coolDown)
    {         
        await System.Threading.Tasks.Task.Delay((int)(coolDown * 1000));
        _executedActions.TryRemove(id, out _);
    }

    public bool TakeAction(string id, Action action, float coolDown = 0.0f)
    {
        if (_executedActions.ContainsKey(id)) return false;

        action();
        _executedActions.TryAdd(id, 0);

        if (coolDown > 0.0f) ScheduleRemove(id, coolDown);

        return true;
    }

    //// returns true if action was executed, false if it was already executed before
    //public bool TakeAction(string id, Action action, float coolDown = 0.0f)
    //{
    //    if (_executedActions.ContainsKey(id)) return false;

    //    action();
    //    _executedActions.TryAdd(id, 0);

    //    if (coolDown > 0.0f) ScheduleRemove(id, coolDown);
        
    //    return true;
    //}
}
