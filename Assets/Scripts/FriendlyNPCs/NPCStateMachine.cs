public class NPCStateMachine
{
    private NPCState currentState;
    public NPCState CurrentState => currentState;

    public void SetState(NPCState newState, BaseNPC npc)
    {
        currentState?.Exit(npc);
        currentState = newState;
        currentState?.Enter(npc);
    }

    public void Update(BaseNPC npc)
    {
        var newstate = currentState?.Update(npc);
        if (newstate != null) SetState(newstate, npc);
    }
}
