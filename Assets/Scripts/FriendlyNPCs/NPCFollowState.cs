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
            agent.speed = 3.5f; // Set speed for following
        }
    }

    public override void Update(FriendlyNPC npc)
    {
        if (npc.leader != null)
        {
            agent?.SetDestination(npc.leader.transform.position);
        }
    }

    public override void Exit(FriendlyNPC npc)
    {
        agent = null;
    }
}
