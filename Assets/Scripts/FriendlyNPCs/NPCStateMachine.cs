public class NPCStateMachine
{
    private NPCState currentState;

    public void SetState(NPCState newState, SonNPC npc)
    {
        currentState?.Exit(npc);
        currentState = newState;
        currentState?.Enter(npc);
    }

    public void Update(SonNPC npc)
    {
        currentState?.Update(npc);
    }
}
