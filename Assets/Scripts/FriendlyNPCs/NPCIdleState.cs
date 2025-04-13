#nullable enable

using UnityEngine;

public class NPCIdleState : NPCState
{
    public NPCIdleState(BaseNPC baseNpc) : base(baseNpc)
    {
        if (baseNpc is not FriendlyNPC)
        {
            throw new System.Exception("BaseNPC is not a FriendlyNPC. Cannot enter idle state.");
        }
    }

    // remove this ctor type
    private NPCIdleState(NPCState? nextState, BaseNPC? npc = null) {}

    public override void Enter()
    {
        // nothing to do
    }

    public override INPCState? Update()
    {
        if (this.NPC.target == null) return null;
        
        // turn in the direction of the target
        Vector3 direction = this.NPC.target.position - this.NPC.transform.position;
        direction.y = 0f; // Keep rotation flat
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            this.NPC.transform.rotation = Quaternion.RotateTowards(
                this.NPC.transform.rotation,
                targetRotation,
                this.NPC.rotationSpeed * Time.deltaTime
            );
        }

        return null;
    }

    public override void Exit()
    {
        // nothing to do
    }
}
