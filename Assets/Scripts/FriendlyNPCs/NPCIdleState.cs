using UnityEngine;

public class NPCIdleState : NPCState
{
    public override void Enter(FriendlyNPC npc)
    {
        // Debug or animation trigger if needed
    }

    public override void Update(FriendlyNPC npc)
    {
        if (npc.leader == null)
            return;

        Vector3 direction = npc.leader.position - npc.transform.position;
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
    }

    public override void Exit(FriendlyNPC npc)
    {
        // Clean up if needed
    }
}
