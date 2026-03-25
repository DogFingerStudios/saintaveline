#nullable enable

using UnityEngine;

/// <summary>
/// This class is attached to root NPC objects
/// </summary>
public class EnemyNPC : BaseNPC
{
    [SerializeField, NPCStateDropdown]
    private string _defaultState = "EnemyIdle";

    public Transform[] PatrolPoints = new Transform[0];
    public float ArrivalThreshold = 0.5f;

    public float ViewAngle = 120f;
    public Vector3 EyeOffset = new(0f, 1.6f, 0f);

    protected override void Start()
    {
        base.Start();

        var state = NPCStateFactory.CreateState(_defaultState, this);
        if (state != null)
        {
            this.stateMachine.SetState(state);
        }

        MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
    }

    public override void HandleSound(SoundStimulus stim)
    {
        base.HandleSound(stim);

        if (stim.Kind == StimulusKind.Gunshot)
        {
            this.stateMachine.CurrentState?.HandleSound(stim);
        }
    }

    private void OnMissionCompleted(Mission mission)
    {
        if (mission.State == MissionState.Failed)
        {
            if (this.StateMachine.CurrentState != null)
            {
                this.StateMachine.CurrentState!.IsAggro = true;
            }
        }
    }
}