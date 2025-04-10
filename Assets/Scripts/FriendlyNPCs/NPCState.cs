#nullable enable

public abstract class NPCState
{
    public abstract void Enter(FriendlyNPC npc);
    public abstract void Exit(FriendlyNPC npc);
    public abstract NPCState? Update(FriendlyNPC npc);
}
