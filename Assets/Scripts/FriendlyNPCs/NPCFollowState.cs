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
