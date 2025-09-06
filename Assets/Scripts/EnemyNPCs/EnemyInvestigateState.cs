#nullable enable

using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;

[NPCStateTag("EnemyInvestigateState")]
public class EnemyInvestigateState : NPCState
{
    private NavMeshAgent _agent;
    private readonly EnemyNPC _enemyNPC;
    private readonly Vector3 _investigationPoint;
    private readonly Vector3 _originalPosition;
    
    private EntityScanner _entityScanner;
    private float _timer = 0f;
    private readonly float _scanInterval = 0.5f;

    private readonly int _targetMask = LayerMask.GetMask("Player", "FriendlyNPC");
    private readonly int _obstacleMask = LayerMask.GetMask("Default");

    private Stack<Vector3> _patrolPoints = new Stack<Vector3>();

    public EnemyInvestigateState(EnemyNPC enemyNPC, Vector3 investigationPoint) 
        : base(enemyNPC)
    {
        _enemyNPC = enemyNPC;
        _agent = this.NPC.GetComponent<NavMeshAgent>();

        _patrolPoints.Push(enemyNPC.transform.position);
        _patrolPoints.Push(investigationPoint);

        _entityScanner = new EntityScanner
        {
            ViewDistance = _enemyNPC.DetectionDistance,
            ViewAngle = _enemyNPC.ViewAngle,
            SourceTransform = this.NPC.transform,
            EyeOffset = _enemyNPC.EyeOffset,
            TargetMask = _targetMask,
            ObstacleMask = _obstacleMask
        };        
    }

    public override void Enter()
    {
        _agent.SetDestination(_patrolPoints.Pop());
    }

    public override void Exit()
    {
        // nothing to do
    }

    public override NPCStateReturnValue? Update()
    {
        if (!_agent.pathPending && _agent.remainingDistance < _enemyNPC.ArrivalThreshold)
        {
            if (_patrolPoints.Count > 0)
            {
                _agent.SetDestination(_patrolPoints.Pop());
            }
            else
            {
                return new NPCStateReturnValue(
                    NPCStateReturnValue.ActionType.PopState
                );
            }
        }

        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            var target = _entityScanner.doScan(1).FirstOrDefault();
            if (target != null)
            {
                var targetHealth = target.GetComponent<IHasHealth>();
                if (targetHealth != null && targetHealth.IsAlive)
                {
                    // this.NPC!.PushState(this);
                    return new NPCStateReturnValue(
                            NPCStateReturnValue.ActionType.ChangeState,
                            new EnemyPursueState(this.NPC, target.transform));
                }
            }
            _timer = 0f;
        }        

        return null;
    }
}