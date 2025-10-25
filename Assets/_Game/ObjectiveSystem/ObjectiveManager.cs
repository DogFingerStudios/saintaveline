using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private ObjectiveSO InitialObjective;

    private ObjectiveSystem _objectiveSystem = ObjectiveSystem.Instance;

    public void Awake()
    {
        var player = GameObject.FindWithTag("Player");
        var entity = player.GetComponent<CharacterEntity>();

        _objectiveSystem.CurrentObjective = 
            ObjectiveFactory.Instance.CreateObjectiveFromSO(InitialObjective, entity);
        
        _objectiveSystem.ManualAwake();
    }

    void Update()
    {
        _objectiveSystem.ManualUpdate();
    }
}
