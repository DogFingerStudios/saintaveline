#nullable enable
using System;
using UnityEngine;

[NPCStateTag("EnemyFollow")]
public class EnemyFollowState : NPCState
{
    private UnityEngine.AI.NavMeshAgent _agent = null!;
    private AudioClip? _getBackInsideSound;

    // AI: Controls how often the destination is updated (in seconds)
    private const float _destinationUpdateInterval = 1.0f;
    private Vector3 _lastTargetPosition = Vector3.zero;
    private float _lastDestinationUpdateTime = 0f;

    private readonly GameEntity _targetEntity;

    // TODO: this is a poor man's way to stop chasing, eventually we will want to be a 
    // little smarter -- for example, if the NPC cannot "see" the Target, then the NPC could
    // go to the last position it saw the Target, and if the Target is not in range or
    // not visible, then the NPC could return to previous state
    private float _detectionRange;

    /// <summary>
    ///  how long we've been following the target
    /// </summary>
    private float _followStopTime = 0f;
    private readonly float FollowTimeout = 30f; // how long to follow before attacking

    /// <param name="npc">The NPC to which this state is attached.</param>
    /// <param name="target">The Target Transform that the NPC will pursue.</param>
    public EnemyFollowState(BaseNPC npc, GameEntity target)
        : base(npc)
    {
        // TODO: CHANGE ME!!
        this.NPC!.Target = target.transform;

        if (this.NPC is not EnemyNPC)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter pursue state.");
        }

        _targetEntity = this.NPC!.Target!.GetComponent<GameEntity>();
        _getBackInsideSound = Resources.Load<AudioClip>("Sounds/GetBackInside");
    }

    public override void Enter()
    {
        if (!this.NPC!.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out _agent))
        {
            throw new System.Exception("NavMeshAgent component is missing on the NPC.");
        }

        _detectionRange = this.NPC.DetectionDistance;
        _followStopTime = Time.time + FollowTimeout;

        this.NPC!.Partnership.TakeAction("getbackinside", 
            () => this.NPC!.AudioSource.PlayOneShot(_getBackInsideSound), 5);
    }

    public override void Exit()
    {
        // nothing to do
    }

    private void SetDestination(Vector3 targetPosition)
    {
        if (_lastTargetPosition == targetPosition) return;
        if (Time.time < _lastDestinationUpdateTime + _destinationUpdateInterval) return;

        _agent!.SetDestination(targetPosition);
        _lastDestinationUpdateTime = Time.time;
        _lastTargetPosition = targetPosition;
    }

    public override NPCStateReturnValue? Update()
    {
        if (!_targetEntity!.IsAlive)
        {
            // Target is dead, go back to idle state
            _agent.isStopped = true;
            _agent.ResetPath();

            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.PopState);
        }

        if (Time.time >= _followStopTime)
        {
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.ChangeState,
                    new EnemyPursueState(this.NPC!, _targetEntity));
        }

        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.Target.position);
        if (distance <= _detectionRange)
        {
            this.SetDestination(this.NPC.Target.position);
        }
        else
        {
            // the target is out of range
            _agent.isStopped = true;
            _agent.ResetPath();

            // Target is out of range, go back to idle state which we pushed earlier
            return new NPCStateReturnValue(
                NPCStateReturnValue.ActionType.PopState);
        }

        return null;
    }
}
