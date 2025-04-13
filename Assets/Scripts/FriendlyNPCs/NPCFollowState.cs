#nullable enable

using UnityEngine;

public class NPCFollowState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? _agent;

    public NPCFollowState(BaseNPC baseNpc) : base(baseNpc)
    {
        if (baseNpc is not FriendlyNPC friendlyNPC)
        {
            throw new System.Exception("BaseNPC is not a FriendlyNPC. Cannot enter idle state.");
        }

        if (NPC.target == null)
        {
            throw new System.Exception("Target is null. Cannot enter follow state.");
        }

        _agent = this.NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent == null)
        {
            throw new System.Exception("NavMeshAgent component not found on NPC.");
        }
    }

    // remove this ctor type
    private NPCFollowState(NPCState? nextState, BaseNPC? npc = null) {}

    public override void Enter()
    {
        _agent.isStopped = false;
        _agent.speed = this.NPC.moveSpeed;
        _agent.angularSpeed = this.NPC.rotationSpeed;
    }

    public override INPCState? Update()
    {
        float distance = Vector3.Distance(this.NPC.transform.position, this.NPC.target.position);
        if (distance < this.NPC.stopDistance)
        {
            // we're close enough to the target, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();
            return new NPCFollowIdleState(this.NPC);
        }

        if (distance < this.NPC.detectionDistance)
        {
            // we're chasing the target
            _agent.SetDestination(this.NPC.target.transform.position);
        }
        else
        {
            // the target is out of range, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();
            return new NPCFollowIdleState(this.NPC);
        }

        return null;
    }

    public override void Exit()
    {
        _agent.ResetPath();
        _agent = null;
    }
}
