using UnityEngine;

public class EnemyNPC : BaseNPC
{
    [SerializeField, NPCStateDropdown] 
    private string _defaultState;

    public Transform[] PatrolPoints;
    public float ArrivalThreshold = 0.5f;

    public float ViewDistance = 25f;
    public float ViewAngle = 120f;
    public Vector3 EyeOffset = new(0f, 1.6f, 0f);

    private EnemyPatrolState? _patrolState = null;

    protected override void Start()
    {
        base.Start();

        var state = NPCStateFactory.CreateState(_defaultState, this);
        this.stateMachine.SetState(state);

        if (state is EnemyPatrolState patrolState)
        {
            _patrolState = patrolState;
        }
    }
}
