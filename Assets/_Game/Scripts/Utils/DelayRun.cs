using System;
using System.Threading;
using System.Collections;
using UnityEngine;

public static class DelayRun
{
    public static Coroutine Do(MonoBehaviour behaviour, System.Action action, float seconds)
    {
        if (behaviour == null || action == null) return null;
        return behaviour.StartCoroutine(RunAfter(action, seconds));
    }

    private static IEnumerator RunAfter(System.Action action, float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }

        action?.Invoke();
    }
}
