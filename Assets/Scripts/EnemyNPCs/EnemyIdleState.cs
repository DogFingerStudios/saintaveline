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

    private AudioClip? _warningSound;
    private AudioClip? _willFindYouSound;
    
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
            ViewDistance = _enemyNPC.ViewDistance,
            ViewAngle = _enemyNPC.ViewAngle,
            SourceTransform = this.NPC!.transform,
            EyeOffset = _enemyNPC.EyeOffset,
            TargetMask = _targetMask,
            ObstacleMask = _obstacleMask
        };

        _warningSound = Resources.Load<AudioClip>("Sounds/Freeze");
        _willFindYouSound = Resources.Load<AudioClip>("Sounds/IWillFindYou");
    }

    public override void Enter()
    {
        _originalDirection = this.NPC!.transform.forward.normalized;
    }

    public override INPCState? Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            var target = doScan();
            if (target != null)
            {
                this.NPC!.target = target.transform;

                // turn in the direction of the target
                Vector3 direction = this.NPC!.target.position - this.NPC!.transform.position;
                _currentTargetDirection = direction.normalized;

                // Play audio clip named "Freeze"
                if (!_hasPlayedWarningSound && this.NPC!.AudioSource != null && _warningSound != null)
                {
                    this.NPC!.AudioSource.PlayOneShot(_warningSound);
                    _hasPlayedWarningSound = true;
                }
                else if (!_hasPlayedWarningSound)
                {
                    Debug.LogWarning("Cannot play warning sound: AudioSource or warningSound is missing on NPC.");
                }

            }
            else if (_originalDirection != this.NPC!.transform.forward.normalized)
            {
                _currentTargetDirection = _originalDirection;
                if (_hasPlayedWarningSound && this.NPC!.AudioSource != null && _willFindYouSound != null)
                {
                    this.NPC!.AudioSource.PlayOneShot(_willFindYouSound);
                    _hasPlayedWarningSound = false;
                }
            }

            _timer = 0f;
        }

        if (_currentTargetDirection != this.NPC!.transform.forward.normalized)
        {
            turnTowards(_currentTargetDirection);        
        }
        
        return null; 
    }

    public override void Exit()
    {
        // nothing to do
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
