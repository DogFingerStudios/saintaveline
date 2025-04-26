using UnityEngine;

public class EnemyNPC : BaseNPC
{
     public Transform[] PatrolPoints;
     public float ArrivalThreshold = 0.5f;

    public float ViewDistance = 25f;
    public float ViewAngle = 120f;
    public Vector3 EyeOffset = new Vector3(0f, 1.6f, 0f);

     private EnemyPatrolState _patrolState;

    protected override void Start()
    {
        // base.Start();
        _patrolState = new EnemyPatrolState(this);
        // this.stateMachine.SetState(_patrolState);
        this.stateMachine.SetState(new EnemyIdleState(this));
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        // Match your detection logic
        Vector3 boxCenter = transform.position + transform.forward * (ViewDistance / 2f);
        Vector3 boxHalfExtents = new Vector3(ViewDistance / 2f, 10f, ViewDistance / 2f); // Example size
        Quaternion rotation = transform.rotation;

        // Visualize as a wire cube
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f); // Multiply by 2 because OverlapBox uses half-extents
    }

}
