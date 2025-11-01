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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FixUnityLayoutBug();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void FixUnityLayoutBug()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(TaskListParent);
    }
}
