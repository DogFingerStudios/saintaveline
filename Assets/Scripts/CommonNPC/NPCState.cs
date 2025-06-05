#nullable enable

public abstract class NPCState
{
    private NPCState? _nextState;
    public NPCState? NextState
    {
        get => _nextState;
        set => _nextState = value;
    }

    private BaseNPC? _npc;
    public BaseNPC? NPC 
    {
        get => _npc;
        set => _npc = value;
    }

    public NPCState(BaseNPC? npc = null)
    {
        if (npc != null) _npc = npc;
    }

    public NPCState(NPCState? nextState, BaseNPC? npc = null)
    {
        _nextState = nextState;
        _npc = npc;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract NPCState? Update();
}
