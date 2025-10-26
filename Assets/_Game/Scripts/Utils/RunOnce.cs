using System;
using UnityEngine;

//Example:
//
//private System.Action runSetup;
//
//private void Awake()
//{
//    runSetup = Once.Do(() =>
//    {
//        Debug.Log("This runs exactly once!");
//        InitializeExpensiveStuff();
//        LoadData();
//    });
//}
//
//private void Update()
//{
//    runSetup(); // First call: runs. All future calls: do nothing.
//}

public class RunOnce
{
    public System.Action Func = null!;
    public void Run()
    {
        Func?.Invoke();
        Func = null;
    }   
}
