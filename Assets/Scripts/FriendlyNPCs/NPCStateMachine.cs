public class NPCStateMachine
{
    private NPCState currentState;
    public NPCState CurrentState => currentState;

    public void SetState(NPCState newState, FriendlyNPC npc)
    {
        currentState?.Exit(npc);
        currentState = newState;
        currentState?.Enter(npc);
    }

    public void Update(FriendlyNPC npc)
    {
        var newstate = currentState?.Update(npc);
        if (newstate != null) SetState(newstate, npc);
    }
}
