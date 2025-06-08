#nullable enable

using UnityEngine;
using System.Linq;

[NPCStateTag("EnemyIdle")]
public class EnemyIdleState : NPCState
{
    private Vector3 _originalDirection;
    private Vector3 _currentTargetDirection;
    private readonly EnemyNPC _enemyNPC;

    private float _timer = 0f;
    private readonly float _scanInterval = 0.25f;
    private readonly int _targetMask = LayerMask.GetMask("Player", "FriendlyNPC");
    private readonly int _obstacleMask = LayerMask.GetMask("Default");

    bool _hasPlayedWarningSound = false;
    private EntityScanner _entityScanner;

    private Vector3? _defaultPosition = null;
    private UnityEngine.AI.NavMeshAgent? _agent = null;

    public EnemyIdleState(EnemyNPC enemyNPC)
        : base(enemyNPC)
    {
        if (this.NPC == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter idle state.");
        }

        _enemyNPC = enemyNPC;

        _entityScanner = new EntityScanner
        {
            ViewDistance = _enemyNPC.DetectionDistance,
            ViewAngle = _enemyNPC.ViewAngle,
            SourceTransform = this.NPC!.transform,
            EyeOffset = _enemyNPC.EyeOffset,
            TargetMask = _targetMask,
            ObstacleMask = _obstacleMask
        };

        if (_agent == null)
        {
            _agent = this.NPC!.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
    }

    public override void Enter()
    {
        _originalDirection = this.NPC!.transform.forward.normalized;
        if (_defaultPosition == null)
        {
            _defaultPosition = this.NPC!.transform.position;
        }
    }

    public override NPCStateReturnValue? Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            var target = doScan();
            if (target != null)
            {
                this.NPC!.target = target.transform;

                this.NPC.PushState(this);
                return new NPCStateReturnValue(
                        NPCStateReturnValue.ActionType.ChangeState,
                        new EnemyPursueState(this.NPC, target.transform));
            }

            _timer = 0f;
        }

        if (_agent != null)
        {
            var distanceToDefault = Vector3.Distance(this.NPC!.transform.position, _defaultPosition!.Value);
            // TODO: should be a settable threshold?
            if (distanceToDefault > 1f)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_defaultPosition.Value);
            }
            else
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }
        
        return null; 
    }

    public override void Exit()
    {
        if (_agent == null) return;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    private Collider? doScan()
    {
        if (this.NPC == null || this.NPC!.transform == null) return null;
        return _entityScanner.doScan(1).FirstOrDefault();
    }

    private void turnTowards(Vector3 direction)
    {
        direction.y = 0f; // Keep rotation flat
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            this.NPC!.transform.rotation = Quaternion.RotateTowards(
                this.NPC!.transform.rotation,
                targetRotation,
                this.NPC!.rotationSpeed * Time.deltaTime
            );
        }
    }
}
