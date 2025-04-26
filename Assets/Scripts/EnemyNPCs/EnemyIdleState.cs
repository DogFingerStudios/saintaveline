#nullable enable

using UnityEngine;

public class EnemyIdleState : NPCState
{
    private Vector3 _originalDirection;
    private Vector3 _currentTargetDirection;
    private readonly EnemyNPC _enemyNPC;

    private float _timer = 0f;
    private readonly float _scanInterval = 0.25f;
    private readonly int _targetMask = LayerMask.GetMask("Player", "FriendlyNPC");
    private readonly int _obstacleMask = LayerMask.GetMask("Default");
    
    public EnemyIdleState(EnemyNPC enemyNPC) 
        : base(enemyNPC)
    {
        if (this.NPC == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter idle state.");
        }

        _enemyNPC = enemyNPC;
    }

    public override void Enter()
    {
        // get the current direction the NPC is facing
        _originalDirection = this.NPC.transform.forward.normalized;
    }

    public override INPCState? Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _scanInterval)
        {
            var target = doScan();
            if (target != null)
            {
                this.NPC.target = target.transform;

                // turn in the direction of the target
                Vector3 direction = this.NPC.target.position - this.NPC.transform.position;
                _currentTargetDirection = direction.normalized;
            }
            else if (_originalDirection != this.NPC.transform.forward.normalized)
            {
                _currentTargetDirection = _originalDirection;
            }

            _timer = 0f;
        }

        if (_currentTargetDirection != this.NPC.transform.forward.normalized)
        {
            turnTowards(_currentTargetDirection);        
        }
        
        return null; 
    }

    public override void Exit()
    {
        // Nothing to do
    }

    private void turnTowards(Vector3 direction)
    {
        direction.y = 0f; // Keep rotation flat
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            this.NPC.transform.rotation = Quaternion.RotateTowards(
                this.NPC.transform.rotation,
                targetRotation,
                this.NPC.rotationSpeed * Time.deltaTime
            );
        }
    }

    private Collider? doScan()
    {
        if (this.NPC == null || this.NPC.transform == null) return null;

        var eyePosition = this.NPC.transform.position + _enemyNPC.EyeOffset;

        Vector3 boxCenter = this.NPC.transform.position + this.NPC.transform.forward * (_enemyNPC.ViewDistance / 2f);
        Vector3 boxHalfExtents = new Vector3(_enemyNPC.ViewDistance / 2f, 20.5f, _enemyNPC.ViewDistance ); // Flatten vertically if needed

        Collider[] candidates = Physics.OverlapBox(boxCenter, boxHalfExtents, this.NPC.transform.rotation, _targetMask);

        foreach (Collider target in candidates)
        {
            if (target.transform == this.NPC.transform) continue;

            Vector3 dirToTarget = (target.transform.position - eyePosition).normalized;
            float angleToTarget = Vector3.Angle(this.NPC.transform.forward, dirToTarget);
            if (angleToTarget > (_enemyNPC.ViewAngle / 2f)) continue;

            float distanceToTarget = Vector3.Distance(eyePosition, target.transform.position);
            if (!Physics.Raycast(eyePosition, dirToTarget, distanceToTarget, _obstacleMask))
            {
                Debug.Log($"Target: {target.name}, Distance: {distanceToTarget}, Angle: {angleToTarget}");
                return target;
            }
        }

        return null;
    }
}
