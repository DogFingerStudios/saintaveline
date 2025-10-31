using UnityEngine;

public class ArriveAtTask : Task
{
    public float ArrivedDistance => Data.ArrivedDistance;
    public Transform ChracterTransform => Host!.transform;

    private ArriveAtTaskSO Data => this.TypedData<ArriveAtTaskSO>();

    public ArriveAtTask(ArriveAtTaskSO data)
        : base(data)
    {
        // nothing to do
    }

    public override void ManualUpdate()
    {
        if (Vector3.Distance(ChracterTransform.position, Data.Location) <= ArrivedDistance)
        {
            base.Complete();
        }
    }
}
