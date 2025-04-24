using UnityEngine;

public class EnemyNPC : BaseNPC
{
     public Transform[] PatrolPoints;
     public float ArrivalThreshold = 0.5f;

     private EnemyPatrolState _patrolState;

    protected override void Start()
    {
        // base.Start();
        _patrolState = new EnemyPatrolState(this);
        this.stateMachine.SetState(_patrolState);
    }
}
