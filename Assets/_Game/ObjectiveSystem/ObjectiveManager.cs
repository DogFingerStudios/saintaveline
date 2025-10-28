using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private GameObject Minimap;
    [SerializeField] private ObjectiveSO InitialObjective;

    //private ObjectiveSystem _objectiveSystem = ObjectiveSystem.Instance;
    private Objective? CurrentObjective;
    private RunOnce? _runonce;

    public void Awake()
    {
        var player = GameObject.FindWithTag("Player");
        var entity = player.GetComponent<CharacterEntity>();

        CurrentObjective =
            ObjectiveFactory.Instance.CreateObjectiveFromSO(InitialObjective, entity);

        if (CurrentObjective == null) return;

        CurrentObjective.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective.ManualAwake();

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
                }
            };
        }
    }

    void Update()
    {
        _runonce?.Run();
        if (CurrentObjective == null) return;

        CurrentObjective.ManualUpdate();
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
