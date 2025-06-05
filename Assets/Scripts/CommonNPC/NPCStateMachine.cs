public class NPCStateMachine
{
    private NPCState currentState;
    public NPCState CurrentState => currentState;

    public void SetState(NPCState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        var newstate = currentState?.Update();
        if (newstate != null) this.SetState(newstate);
    }
}

