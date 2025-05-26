#nullable enable

// TODO: do we really need/want this interface?
public interface INPCState
{
    void Enter();
    void Exit();
    INPCState? Update();
}

public abstract class NPCState : INPCState
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
    public abstract INPCState? Update();
}
