using System.Collections.Generic;

public class NPCStateMachine
{
    private NPCState currentState;
    public NPCState CurrentState => currentState;

    public Stack<NPCState> StateStack = new Stack<NPCState>();

    public void SetState(NPCState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        if (currentState == null) return;
        NPCStateReturnValue? retval = currentState!.Update();
        if (retval != null)
        {
            switch (retval!.Type1)
            {
                default:
                break;

                case NPCStateReturnValue.ReturnType.NextState:
                    SetState(retval.NextState!);
                break;

                case NPCStateReturnValue.ReturnType.ExitState:
                {
                    if (StateStack.Count > 0)
                    {
                        SetState(StateStack.Pop());
                    }
                    else
                    {
                        currentState = null; // No more states to return to
                    }
                }
                break;
            }
        }
    }
}

