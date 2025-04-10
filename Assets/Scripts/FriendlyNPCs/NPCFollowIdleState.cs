using UnityEngine;

public class NPCFollowIdleState : NPCState
{
    public override void Enter(FriendlyNPC npc)
    {
    }

    public override NPCState? Update(FriendlyNPC npc)
    {
        if (npc.target == null) return null;

        float distance = Vector3.Distance(npc.transform.position, npc.target.position);
        if (distance < npc.detectionDistance)
        {
            // If the target is within detection distance, switch to follow state
            return new NPCFollowState();
        }

        Vector3 direction = npc.target.position - npc.transform.position;
        direction.y = 0f; // Keep rotation flat

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            npc.transform.rotation = Quaternion.RotateTowards(
                npc.transform.rotation,
                targetRotation,
                npc.rotationSpeed * Time.deltaTime
            );
        }

        return null;
    }

    public override void Exit(FriendlyNPC npc)
    {
        // Clean up if needed
    }
}
