#nullable enable

using UnityEngine;

public class NPCFollowState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? agent;
    private FriendlyNPC? npc;

    public void Enter(BaseNPC baseNpc)
    {
        // TODO: this should probably have some good error checking
        if (baseNpc is not FriendlyNPC friendlyNpc) return;
        this.npc = friendlyNpc;
        if (npc.target == null) return;

        agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = npc.moveSpeed;
            agent.angularSpeed = npc.rotationSpeed;
        }
    }

    public NPCState? Update(BaseNPC x)
    {
        if (npc == null || npc.target == null || agent == null) return null;
        
        float distance = Vector3.Distance(npc.transform.position, npc.target.position);
        if (distance < npc.stopDistance)
        {
            // we're close enough to the target, stop moving
            agent.isStopped = true;
            agent.ResetPath();
            return new NPCFollowIdleState();
        }

        if (distance < npc.detectionDistance)
        {
            // we're chasing the target
            agent.SetDestination(npc.target.transform.position);
        }
        else
        {
            // the target is out of range, stop moving
            agent.isStopped = true;
            agent.ResetPath();
            return new NPCFollowIdleState();
        }

        return null;
    }

    public void Exit(BaseNPC x)
    {
        agent?.ResetPath();
        agent = null;
    }
}
