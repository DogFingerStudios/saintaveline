#nullable enable

using UnityEngine;
using System.Linq;

[NPCStateTag("EnemyIdle")]
public class EnemyIdleState : NPCState
{
    private Vector3 _originalDirection;
    private readonly EnemyNPC _enemyNPC;

    private float _timer = 0f;
    private readonly float _scanInterval = 0.5f;
    private readonly int _targetMask = LayerMask.GetMask("Player", "FriendlyNPC");
    private readonly int _obstacleMask = LayerMask.GetMask("Default");

    private EntityScanner _entityScanner;

    private Vector3? _defaultPosition = null;
    private UnityEngine.AI.NavMeshAgent? _agent = null;

    private float _idleOscillationTime = 0f;
    private readonly float _idleOscillationSpeed = 0.5f;
    private readonly float _maxIdleAngle = 45f;

    private NPCState? _nextState = null;
    private bool _firstTime = true;

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
        if (_firstTime)
        {
            _originalDirection = this.NPC!.transform.forward.normalized;
            if (_defaultPosition == null)
            {
                _defaultPosition = this.NPC!.transform.position;
            }
            _firstTime = false;
        }
        else
        {
            turnTowards(_originalDirection);
        }

        _idleOscillationTime = 0f;
    }

    public override NPCStateReturnValue? Update()
    {
        if (_nextState != null)
        {
            this.NPC!.PushState(this);
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState,
                _nextState
            );
        }

        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            var target = doScan();
            if (target != null)
            {
                var targetHealth = target.GetComponent<IHasHealth>();
                if (targetHealth != null && targetHealth.IsAlive)
                {
                    this.NPC!.PushState(this);
                    return new NPCStateReturnValue(
                            NPCStateReturnValue.ActionType.ChangeState,
                            new EnemyPursueState(this.NPC, target.transform));
                }
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
        
        // AI: oscillate the NPC's direction for idle animation
        _idleOscillationTime += Time.deltaTime;
        float angle = Mathf.Sin(_idleOscillationTime * _idleOscillationSpeed) * _maxIdleAngle;
        Vector3 targetDirection = Quaternion.Euler(0f, angle, 0f) * _originalDirection;
        turnTowards(targetDirection);

        return null; 
    }

    public override void Exit()
    {
        _nextState = null;
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

    public override void HandleSound(SoundStimulus stim)
    {
        base.HandleSound(stim);

        if (stim.Kind == StimulusKind.Gunshot)
        {
            _nextState = new EnemyInvestigateState(_enemyNPC, stim.Position);
        }
    }
}
