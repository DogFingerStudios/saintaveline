#nullable enable

using UnityEngine;

public class NPCFollowState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? agent;

    public override void Enter(FriendlyNPC npc)
    {
        agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = npc.moveSpeed;
            agent.angularSpeed = npc.rotationSpeed;
        }
    }

    public override NPCState? Update(FriendlyNPC npc)
    {
        if (npc.target == null || agent == null) return null;
        
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

    public override void Exit(FriendlyNPC npc)
    {
        agent?.ResetPath();
        agent = null;
    }
}
