using UnityEngine;

[NPCStateTag("EnemyPursue")]
public class EnemyPursueState : NPCState
{
    private UnityEngine.AI.NavMeshAgent? _agent = null;

    // TODO: this is a poor man's way to stop chasing, eventually we will want to be a 
    // little smarter -- for example, if the NPC cannot "see" the target, then the NPC could
    // go to the last position it saw the target, and if the target is not in range or
    // not visible, then the NPC could return to patrol state
    private float _detectionRange = 20f;

    public EnemyPursueState(BaseNPC npc, Transform target) 
        : base(npc)
    {
        this.NPC.target = target;
        if (this.NPC is not EnemyNPC)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter pursue state.");
        }
    }

    public override void Enter()
    {
        _agent = this.NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent == null)
        {
            throw new System.Exception("NavMeshAgent component is missing on the NPC.");
        }
    }

    public override void Exit()
    {

    }

    public override INPCState? Update()
    {
        float distance = Vector3.Distance(this.NPC.transform.position, this.NPC.target.position);
        if (distance < this.NPC.stopDistance)
        {
            // we're close enough to the target, stop moving
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        if (distance <= _detectionRange)
        {
            _agent.SetDestination(this.NPC.target.transform.position);
        }
        else
        {
            _agent.ResetPath();
            return new EnemyIdleState((EnemyNPC)this.NPC);
        }

        return null;
    }
}
