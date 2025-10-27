using UnityEngine;

public static class ScriptableObjectExtensions
{
    public static T Copy<T>(this T original) where T : ScriptableObject
    {
        return Object.Instantiate(original) as T;
    }
}