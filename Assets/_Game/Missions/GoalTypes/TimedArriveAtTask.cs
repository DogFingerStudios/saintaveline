using UnityEngine;

public class TimedArriveAtTask : Task
{
    private TimedArriveAtTaskSO Data => this.TypedData<TimedArriveAtTaskSO>();
    public float ArrivedDistance => Data.ArrivedDistance;
    public float TimeLimit => Data.TimeLimit;
    public Transform ChracterTransform => Host!.transform;

    private float _timeLeft = 0f;

    public TimedArriveAtTask(TimedArriveAtTaskSO data)
        : base(data)
    {
        _timeLeft = data.TimeLimit;
    }

    public override void ManualUpdate()
    {
        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            Debug.LogWarning("Mission failed");
            base.Complete(false);
            return;
        }

        var distanceToTarget = Vector3.Distance(ChracterTransform.position, Data.Location);
        if (distanceToTarget <= ArrivedDistance)
        {
            base.Complete();
        }

        if (Data.ShowTimer)
        {
            // yay!
        }
    }
}
