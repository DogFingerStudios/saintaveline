#nullable enable

using UnityEngine;

public class NPCFollowState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? _agent;

    public NPCFollowState(BaseNPC baseNpc) 
        : base(baseNpc)
    {
        if (this.NPC is not FriendlyNPC)
        {
            throw new System.Exception("NPC is not a FriendlyNPC. Cannot enter NPCFollowState state.");
        }

        if (this.NPC.Target == null)
        {
            throw new System.Exception("Target is null. Cannot enter follow state.");
        }

        _agent = this.NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent == null)
        {
            throw new System.Exception("NavMeshAgent component not found on NPC.");
        }
    }

    public override void Enter()
    {
        // TODO: logging?
        if (_agent == null || this.NPC == null) return;

        _agent.isStopped = false;
        _agent.speed = this.NPC.moveSpeed;
        _agent.angularSpeed = this.NPC.rotationSpeed;
    }

    public override NPCStateReturnValue? Update()
    {
        if (_agent == null || this.NPC == null) return null;

        float distance = Vector3.Distance(this.NPC.transform.position, this.NPC.Target.position);
        if (distance < this.NPC.stopDistance)
        {
            // we're close enough to the Target, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();

            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState, 
                new NPCFollowIdleState(this.NPC));
        }

        if (distance < this.NPC.DetectionDistance)
        {
            // we're chasing the Target
            _agent.SetDestination(this.NPC.Target.transform.position);
        }
        else
        {
            // the Target is out of range, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();

            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState, 
                new NPCFollowIdleState(this.NPC));

        }

        return null;
    }

    public override void Exit()
    {
        if (_agent == null) return;
        _agent.ResetPath();
        _agent = null;
    }
}
