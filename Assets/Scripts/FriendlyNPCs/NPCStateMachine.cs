public class NPCStateMachine
{
    private NPCState currentState;

    public void SetState(NPCState newState, FriendlyNPC npc)
    {
        currentState?.Exit(npc);
        currentState = newState;
        currentState?.Enter(npc);
    }

    public void Update(FriendlyNPC npc)
    {
        currentState?.Update(npc);
    }
}
