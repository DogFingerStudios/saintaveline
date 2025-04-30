using UnityEngine;

public class EnemyNPC : BaseNPC
{
     public Transform[] PatrolPoints;
     public float ArrivalThreshold = 0.5f;

    public float ViewDistance = 25f;
    public float ViewAngle = 120f;
    public Vector3 EyeOffset = new(0f, 1.6f, 0f);

     private EnemyPatrolState _patrolState;

    protected override void Start()
    {
        base.Start();
        _patrolState = new EnemyPatrolState(this);
        this.stateMachine.SetState(_patrolState);
    }
}
