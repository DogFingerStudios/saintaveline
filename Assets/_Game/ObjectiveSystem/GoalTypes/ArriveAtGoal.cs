using UnityEngine;

public class ArriveAtGoal : Goal
{
    public Vector3 Location => Data.Location;
    public float ArrivedDistance => Data.ArrivedDistance;
    public Transform ChracterTransform => Host!.transform;

    private ArriveAtGoalSO Data => this.TypedData<ArriveAtGoalSO>();

    public ArriveAtGoal(ArriveAtGoalSO data)
        : base(data)
    {
        // nothing to do
    }

    public override void ManualUpdate()
    {
        if (Vector3.Distance(ChracterTransform.position, Location) <= ArrivedDistance)
        {
            base.Complete();
        }
    }
}
