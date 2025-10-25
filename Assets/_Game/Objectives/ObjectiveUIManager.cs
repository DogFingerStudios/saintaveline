using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveUIManager : MonoBehaviour
{
    private ObjectiveSystem _objectiveSystem = ObjectiveSystem.Instance;

    public void Awake()
    {
        ArriveAtGoal arriveGoal = new(this.transform)
        {
            Name = "Reach Starting Point",
            Description = "Arrive at the designated starting point to begin your journey.",
            Location = new Vector3(374.73999f, 0f, 391),
            ArrivedDistance = 5.0f
        };

        Objective firstObjective = new()
        {
            Name = "Begin Your Journey",
            Description = "Reach the starting point to embark on your adventure.",
            Goals = new Stack<Goal>()
        };

        firstObjective.Goals.Push(arriveGoal);
        ObjectiveSystem.Instance.CurrentObjective = firstObjective;
        ObjectiveSystem.Instance.ManualAwake();
    }

    void Update()
    {
        _objectiveSystem.ManualUpdate();
    }
}
