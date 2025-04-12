#nullable enable

public interface NPCState
{
    void Enter(BaseNPC npc);
    void Exit(BaseNPC npc);
    NPCState? Update(BaseNPC npc);
}
