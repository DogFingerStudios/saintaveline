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

//// Example usage:
////     Delay.Do(() => Debug.Log("Hello after 1.5 sec"), 1500);
//public static class DelayRun
//{
//    public static void Do(Action action, int milliseconds)
//    {
//        if (action == null) return;
//        if (milliseconds <= 0)
//        {
//            action();
//            return;
//        }

//        new Timer(_ =>
//            {
//                UnityMainThreadDispatcher.Instance().Enqueue(action);
//            }, null, milliseconds, Timeout.Infinite);
//    }
//}

//using System.Collections;
//using UnityEngine;

////Example:
////
//// void Update() // some MonoBehaviour object
//// {
////     this.DelayRun.Do(() => Debug.Log("5 sec later"), 5f);
//// }

//public static class DelayRun
//{
//    public static void Do(this MonoBehaviour mb, System.Action action, float seconds)
//    {
//        mb.StartCoroutine(RunAfter(action, seconds));
//    }

//    private static IEnumerator RunAfter(System.Action action, float seconds)
//    {
//        yield return new WaitForSeconds(seconds);
//        action?.Invoke();
//    }
//}