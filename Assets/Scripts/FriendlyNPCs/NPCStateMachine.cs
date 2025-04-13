public class NPCStateMachine
{
    private INPCState currentState;
    public INPCState CurrentState => currentState;

    public void SetState(INPCState newState)
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
