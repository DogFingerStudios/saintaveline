using System;
using UnityEngine;


public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private RectTransform MinimapUIObject;
    [SerializeField] private ObjectiveSO InitialObjective;    

    // we assume that the minimap camera is a child of the minimap object
    [SerializeField] private Camera MinimapCamera;

    private Objective? CurrentObjective;
    
    private RunOnce? _runonce;
    private RunOnce _init;

    public void Awake()
    {
        if (MinimapCamera == null)
        {
            throw new Exception("Minimap camera not assigned.");
        }

        if (InitialObjective == null)
        {
            throw new Exception("InitialObjective not assigned.");
        }

        _init = new RunOnce()
        {
            PreCalls = 1,
            Func = () => Initialization()
        };
    }

    // This function will be called in `Update()`
    void Initialization()
    {
        var player = GameObject.FindWithTag("Player");
        var entity = player.GetComponent<CharacterEntity>();

        ObjectiveConfig config = new()
        {
            Host = entity,
            MinimapCamera = MinimapCamera,
            MinimapParent = MinimapUIObject
        };

        CurrentObjective =
            ObjectiveFactory.Instance.CreateObjectiveFromSO(InitialObjective, config);

        if (CurrentObjective == null)
        {
            throw new Exception("CurrentObjective is null after creation in Initialization.");
        }

        CurrentObjective.OnObjectiveCompleted += ObjectiveCompleteHandler;
        CurrentObjective.StartObjective();
    }

    void Update()
    {
        _init.Run();

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
