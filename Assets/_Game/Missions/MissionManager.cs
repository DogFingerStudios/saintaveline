#nullable enable
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private RectTransform MinimapUIObject;
    [SerializeField] private MissionSO InitialMission;
    [SerializeField] private MissionOverlayController MissionOverlay;

    // we assume that the minimap camera is a child of the minimap object
    [SerializeField] private Camera MinimapCamera;

    Mission? CurrentMission;
    RunOnce? _runonce;
    RunOnce _init;

    public void Awake()
    {
        if (MinimapCamera == null)
        {
            throw new Exception("Minimap camera not assigned.");
        }

        if (InitialMission == null)
        {
            throw new Exception("InitialMission not assigned.");
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

        MissionConfig config = new()
        {
            Host = entity,
            MinimapCamera = MinimapCamera,
            MinimapParent = MinimapUIObject
        };

        CurrentMission =
            MissionFactory.Instance.CreateMissionFromSO(InitialMission, config);

        if (CurrentMission == null)
        {
            throw new Exception("CurrentMission is null after creation in Initialization.");
        }

        CurrentMission.OnMissionCompleted += MissionCompleteHandler;
        CurrentMission.OnTaskStarted += TaskStartedHandler;
        CurrentMission.OnTaskCompleted += TaskCompletedHandler;

        MissionOverlay.AddMission(CurrentMission);
        CurrentMission.StartMission();
    }

    void Update()
    {
        _init.Run();

        if (CurrentMission == null) return;

        CurrentMission.ManualUpdate();
    }

    void MissionCompleteHandler()
    {
        if (CurrentMission == null)
        {
            throw new Exception("CurrentMission is null in MissionCompleteHandler.");
        }

        CurrentMission = null;
    }

    void TaskStartedHandler(Task task)
    {
        MissionOverlay.AddTask(task);
    }

    void TaskCompletedHandler(Task task)
    {
        MissionOverlay.CompleteTask(task);
    }
}
