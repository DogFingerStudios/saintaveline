#nullable enable

using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : NPCState
{
    private readonly Transform[] _waypoints;
    private float _arrivalThreshold = 0.5f;

    private NavMeshAgent _agent;
    private int _currentIndex = 0;

    public EnemyPatrolState(EnemyNPC enemyNPC) 
        : base(enemyNPC)
    {
        if (this.NPC == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter patrol state.");
        }

        _waypoints = enemyNPC.PatrolPoints;
        _arrivalThreshold = enemyNPC.ArrivalThreshold;
        _agent = this.NPC.GetComponent<NavMeshAgent>();

        if (_waypoints.Length > 0)
        {
            _agent.SetDestination(_waypoints[_currentIndex].position);
        }
    }

    public override void Enter()
    {
        // nothing to do
    }

    public override INPCState? Update()
    {
        if (this.NPC == null) return null;

        if (!_agent.pathPending && _agent.remainingDistance < _arrivalThreshold)
        {
            _currentIndex = (_currentIndex + 1) % _waypoints.Length;
            _agent.SetDestination(_waypoints[_currentIndex].position);
        }

        return null;
    }

    public override void Exit()
    {
        // nothing to do
    }
}
