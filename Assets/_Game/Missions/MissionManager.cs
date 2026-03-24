#nullable enable
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private RectTransform MinimapUIObject = null!;
    [SerializeField] private MissionSO InitialMission = null!;
    [SerializeField] private MissionOverlayController MissionOverlay = null!;

    // we assume that the minimap camera is a child of the minimap object
    [SerializeField] private Camera MinimapCamera = null!;
    [SerializeField] private TextMeshProUGUI TaskTimerText = null!;

    Mission? CurrentMission;
    RunOnce _init = null!;

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

        CurrentMission.OnMissionStarted += MissionStartedHandler;
        CurrentMission.OnMissionCompleted += MissionCompleteHandler;

        CurrentMission.OnTaskStarted += TaskStartedHandler;
        CurrentMission.OnTaskCompleted += TaskCompletedHandler;
        CurrentMission.OnTaskTick += TaskTickHandler;
        
        MissionOverlay.AddMission(CurrentMission);
        CurrentMission.StartMission();
    }

    void Update()
    {
        _init.Run();

        if (CurrentMission == null) return;

        CurrentMission.ManualUpdate();
    }

    void MissionStartedHandler(Mission mission)
    {
        if (mission.StartMessage != string.Empty)
        {
            BottomTypewriter.Instance.Enqueue(mission.StartMessage);
        }
    }

    void MissionCompleteHandler(Mission mission)
    {
        if (CurrentMission == null)
        {
            throw new Exception("CurrentMission is null in MissionCompleteHandler.");
        }

        if (mission.State == MissionState.Completed 
                && !mission.SuccessMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(mission.SuccessMessage);
        }
        else if (mission.State == MissionState.Failed 
                && !mission.FailureMessage.Equals(string.Empty))
        {
            BottomTypewriter.Instance.Enqueue(mission.FailureMessage);
        }

        CurrentMission = null;
    }

    void TaskStartedHandler(Task task)
    {
        MissionOverlay.AddTask(task);
        TaskTimerText.enabled = task is TimedArriveAtTask;
    }

    void TaskCompletedHandler(Task task)
    {
        MissionOverlay.CompleteTask(task);
        TaskTimerText.enabled = false;
    }

    private void TaskTickHandler(Task task)
    {
        if (TaskTimerText != null && task is TimedArriveAtTask taat)
        {
            TaskTimerText.SetText(taat.TimeLeftFormatted);
        }
    }
}
