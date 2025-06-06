#nullable enable

using System.Collections.Generic;

public class NPCStateMachine
{
    private NPCState? currentState;
    public NPCState? CurrentState => currentState;

    public Stack<NPCState> StateStack = new();

    public void SetState(NPCState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState!.Enter();
    }

    public void Update()
    {
        if (currentState == null) return;
        NPCStateReturnValue? retval = currentState!.Update();
        if (retval != null)
        {
            switch (retval!.Action)
            {
                default:
                break;

                case NPCStateReturnValue.ActionType.ChangeState:
                    SetState(retval.NextState!);
                break;

                case NPCStateReturnValue.ActionType.PopState:
                {
                    if (StateStack.Count > 0)
                    {
                        SetState(StateStack.Pop());
                    }
                    else
                    {
                        currentState = null;
                    }
                }
                break;
            }
        }
    }
}

