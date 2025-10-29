using System;
using UnityEngine;


public class ObjectiveManager : MonoBehaviour
{
    //[SerializeField] private GameObject Minimap;
    [SerializeField] private RectTransform MinimapUIObject;
    [SerializeField] private ObjectiveSO InitialObjective;    

    // we assume that the minimap camera is a child of the minimap object
    [SerializeField] private Camera MinimapCamera;

    //private ObjectiveSystem _objectiveSystem = ObjectiveSystem.Instance;
    private Objective? CurrentObjective;
    private RunOnce? _runonce;

    GameObject _activeGoalIcon;

    private void RefactorMeButThisWillDoForNow()
    {
        var goaldata = CurrentObjective?.CurrentGoal?.TypedData<ArriveAtGoalSO>();
        if (goaldata == null) return;

        _activeGoalIcon = Instantiate(goaldata.MinimapIcon, goaldata.Location, goaldata.MinimapIcon.transform.rotation);
        if (_activeGoalIcon.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;
        _activeGoalIcon.GetComponent<GoalIconController>().SetData(MinimapCamera, MinimapUIObject);
    }

    public void Awake()
    {
        if (MinimapCamera == null)
        {
            throw new Exception("Minimap camera not assigned.");
        }

        var player = GameObject.FindWithTag("Player");
        var entity = player.GetComponent<CharacterEntity>();

        CurrentObjective =
            ObjectiveFactory.Instance.CreateObjectiveFromSO(InitialObjective, entity);

        if (CurrentObjective == null) return;

        CurrentObjective.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective.StartObjective();

        if (!CurrentObjective.StartMessage.Equals(string.Empty))
        {
            _runonce = new RunOnce()
            {
                PreCalls = 1,
                Func = () =>
                {
                    BottomTypewriter.Instance.Enqueue(CurrentObjective.StartMessage);

                    if (CurrentObjective.CurrentGoal != null &&
                        !CurrentObjective.CurrentGoal.StartMessage.Equals(string.Empty))
                    {
                        BottomTypewriter.Instance.Enqueue(CurrentObjective.CurrentGoal.StartMessage);
                    }

                    RefactorMeButThisWillDoForNow();
                }
            };
        }
    }

    void Update()
    {
        _runonce?.Run();
        if (CurrentObjective == null) return;

        CurrentObjective.ManualUpdate();

        var goaldata = CurrentObjective?.CurrentGoal?.TypedData<ArriveAtGoalSO>();
        Vector3 newPosition = goaldata.Location;
        newPosition.y = MinimapCamera.transform.position.y - 10;
        _activeGoalIcon.transform.position = newPosition;
    }

    void ObjectiveCompleteHandler()
    {
        if (CurrentObjective == null)
        {
            throw new Exception("CurrentObjective is null in ObjectiveCompleteHandler.");
        }

        string msg = $"Completed objective '{CurrentObjective.Name}'";
        Debug.Log(msg);

        if (!CurrentObjective.SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(CurrentObjective.SuccessMessage);
        }

        CurrentObjective = null;
    }
}
