#nullable enable

using UnityEngine;

// The NPCIdleState never leaves here "voluntarily". The only time and NPC will leave
// from this state is when it is told to by the player or it dies.
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

    public override NPCStateReturnValue? Update()
    {
        if (this.NPC == null || this.NPC.Target == null) return null;
        
        // turn in the direction of the Target
        Vector3 direction = this.NPC.Target.position - this.NPC.transform.position;
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
