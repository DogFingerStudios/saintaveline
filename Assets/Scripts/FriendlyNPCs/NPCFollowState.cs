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
        if (npc.target != null && agent != null)
        {
            float distance = Vector3.Distance(npc.transform.position, npc.target.position);
            if (distance < npc.detectionDistance)
            {
                agent.SetDestination(npc.target.transform.position);
            }
            else
            {
                agent.ResetPath();
                return new NPCFollowIdleState();
            }
        }

        return null;
    }

    public override void Exit(FriendlyNPC npc)
    {
        agent?.ResetPath();
        agent = null;
    }
}
