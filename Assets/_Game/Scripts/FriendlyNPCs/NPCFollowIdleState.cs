#nullable enable

using UnityEngine;

public class NPCFollowIdleState : NPCState
{
    public NPCFollowIdleState(BaseNPC baseNpc) 
        : base(baseNpc)
    {
        if (this.NPC is not FriendlyNPC)
        {
            throw new System.Exception("BaseNPC is not a FriendlyNPC. Cannot enter idle state.");
        }

        if (this.NPC.Target == null)
        {
            throw new System.Exception("Target is null. Cannot enter follow state.");
        }
    }

    // remove this ctor type
    private NPCFollowIdleState(NPCState? nextState, BaseNPC? npc = null) {}

    public override void Enter()
    {
        // nothing to do
    }

    public override NPCStateReturnValue? Update()
    {
        // TODO: logging?
        if (this.NPC == null || this.NPC.Target == null) return null;

        float distance = Vector3.Distance(this.NPC.transform.position, this.NPC.Target.position);
        if (distance > this.NPC.stopDistance && distance < this.NPC.DetectionDistance)
        {
            // if the Target is within detection distance, switch to follow state
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState, 
                new NPCFollowState(this.NPC));
        }

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
