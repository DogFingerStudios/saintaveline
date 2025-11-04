using UnityEngine;
using System;

public class InputProcessor
{
    public Action activeInputHandler;
    public Action storedInputHandler;

    public void ActivateInputHandler()
    {
        activeInputHandler = storedInputHandler;
    }

    public void DeactivateInputHandler()
    {
        activeInputHandler = null;
    }

    public void ProcessInput()
    {
        activeInputHandler?.Invoke();
    }
}
