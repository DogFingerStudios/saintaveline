using System;
using UnityEngine;

public class RunOnce
{
    public System.Action Func = null!;

    // How many times Run() must be called before Func is invoked
    public int PreCalls = 0;
    private int _preCallCount = 0;

    // Run() should be called repeatedly (e.g. in Update).
    public void Run()
    {
        // premture optimization? the majority of times that Run() is called,
        // Func will have already been called. So we want to avoid incrementing
        // _preCallCount unnecessarily, hence we have two if-statements. This
        // is ok since the number of times both if-statements are executed is
        // still O(n) in the worst case.
        if ((_preCallCount > PreCalls) || Func == null) return;

        _preCallCount++;

        if (_preCallCount < PreCalls) return;

        Func.Invoke();
        Func = null;
    }   
}
