#nullable enable

using UnityEngine;

public class NPCGoToState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? _agent;
    private Vector3 _targetPosition;

    public NPCGoToState(BaseNPC baseNpc, Vector3 targetPosition) 
        : base(baseNpc)
    {
        this._targetPosition = targetPosition;

        if (this.NPC is not FriendlyNPC)
        {
            throw new System.Exception("NPC is not a FriendlyNPC. Cannot enter NPCFollowState state.");
        }

        _agent = this.NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent == null)
        {
            throw new System.Exception("NavMeshAgent component not found on NPC.");
        }
    }

    private NPCGoToState(NPCState? nextState, BaseNPC? npc = null) {}

    public override void Enter()
    {
        if (_agent == null || this.NPC == null) return;

        _agent.isStopped = false;
        _agent.speed = this.NPC.moveSpeed;
        _agent.angularSpeed = this.NPC.rotationSpeed;
        _agent.SetDestination(_targetPosition);
    }

    public override INPCState? Update()
    {
        if (_agent == null || this.NPC == null || _targetPosition == null)
        {
            return null;
        }

        float distance = Vector3.Distance(this.NPC.transform.position, _targetPosition);
        if (distance < this.NPC.stopDistance)
        {
            // we're close enough to the target, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();
            return new NPCIdleState(this.NPC);
        }

        return null; 
    }

    public override void Exit()
    {
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }
}