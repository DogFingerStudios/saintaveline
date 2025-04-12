#nullable enable

using UnityEngine;

public class NPCIdleState : NPCState
{
    private FriendlyNPC? npc;

    public void Enter(BaseNPC baseNpc)
    {
        // TODO: this should probably have some good error checking
        if (baseNpc is not FriendlyNPC friendlyNpc) return;
        this.npc = friendlyNpc;
        if (npc.target == null) return;
    }

    public NPCState? Update(BaseNPC x)
    {
        // turn in the direction of the target
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

    public void Exit(BaseNPC x)
    {
        // Clean up if needed
    }
}
