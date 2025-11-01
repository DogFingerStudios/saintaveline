using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionOverlayController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI MissionTitle;
    [SerializeField] public TextMeshProUGUI MissionDescription;
    [SerializeField] public TextMeshProUGUI TaskItemPrefab;
    [SerializeField] public RectTransform   TaskListParent;
    [SerializeField] public GameObject      MissionOverlayPanel;

    [SerializeField] private Color TaskCompleteColor = Color.green;
    [SerializeField] private Color TaskFailedColor = Color.red; // TODO: implement task failed state

    Dictionary<Task, (TextMeshProUGUI, TaskLabelController)> _overlayTasks = new();

    public void AddMission(Mission mission)
    {
        MissionTitle.text = mission.Name;
        MissionDescription.text = mission.Data.ShortDescription;
        FixUnityLayoutBug();
    }

    public void AddTask(Task task)
    {
        TextMeshProUGUI newTaskItem = Instantiate(
            TaskItemPrefab, MissionOverlayPanel.transform);

        var labelCtrl = newTaskItem.GetComponent<TaskLabelController>();
        if (labelCtrl == null)
        {
            throw new System.Exception("TaskItemPrefab is missing TaskLabelController component.");
        }

        labelCtrl.Init();
        labelCtrl.CompletedColor = TaskCompleteColor;
        labelCtrl.FailedColor = TaskFailedColor;
        labelCtrl.SetText(task.Name, TaskState.InProgress);

        _overlayTasks[task] = (newTaskItem, labelCtrl);

        FixUnityLayoutBug();
    }

    // TODO: tasks will eventually have states (completed, failed, in-progress)
    public void CompleteTask(Task task)
    {
        var ctrl = _overlayTasks[task].Item2;
        ctrl.SetText(task.Name, TaskState.Completed);
        FixUnityLayoutBug();
    }

    void FixUnityLayoutBug()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(TaskListParent);
    }
}
