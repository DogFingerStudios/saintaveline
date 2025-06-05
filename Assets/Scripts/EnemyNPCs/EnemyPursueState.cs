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

    /// <param name="npc">The NPC to which this state is attached.</param>
    /// <param name="target">The target Transform that the NPC will pursue.</param>
    public EnemyPursueState(NPCState nextState, BaseNPC npc, Transform target) 
        : base(nextState, npc)
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

    public override NPCState? Update()
    {
        float distance = Vector3.Distance(this.NPC.transform.position, this.NPC.target.position);
        if (distance < this.NPC.stopDistance) 
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            // return new EnemyAttackState(this.NPC);
        }

        if (distance <= _detectionRange)
        {
            _agent.SetDestination(this.NPC.target.transform.position);
        }
        else
        {
            _agent.ResetPath();
            return this.NextState; 
        }

        return null;
    }
}
