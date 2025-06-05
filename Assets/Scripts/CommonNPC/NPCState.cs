#nullable enable

public class NPCStateReturnValue
{
    public enum ActionType
    {
        ChangeState,
        PopState
    } ActionType;

    public ActionType Action;
    public NPCState? NextState;
}

public abstract class NPCState
{
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
    public abstract NPCStateReturnValue? Update();
}
