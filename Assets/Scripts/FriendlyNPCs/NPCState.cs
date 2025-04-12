#nullable enable
public interface INPCState
{
    void Enter();
    void Exit();
    NPCState? Update();
}

 public class NPCState : INPCState
{
    protected FriendlyNPC? npc;

    public abstract NPCState(BaseNPC baseNpc);
    public abstract void Enter();
    public abstract void Exit();
    public abstract NPCState? Update();
}
