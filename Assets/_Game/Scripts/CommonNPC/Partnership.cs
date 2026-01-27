using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Partnership
{
    [SerializeField]
    public List<BaseNPC> Partners = new();

    private HashSet<string> executedActions = new();

    // returns true if action was executed, false if it was already executed before
    public bool TakeAction(string id, Action action)
    {
        if (executedActions.Contains(id)) return false;

        action();
        executedActions.Add(id);
        return true;
    }
}
