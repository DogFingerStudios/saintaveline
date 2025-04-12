#nullable enable

using UnityEngine;

public class NPCIdleState : NPCState
{
    private FriendlyNPC? _npc;

    NPCIdleState(BaseNPC baseNpc)
    {
        if (baseNpc is not FriendlyNPC friendlyNpc) return;
        this._npc = friendlyNpc;

        if (_npc.target == null)
        {
            throw new System.Exception("NPC target is null. Please set a target before entering the idle state.");
        }
    }

    public void Enter()
    {
        // nothing to do
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
        // nothing to do
    }
}
