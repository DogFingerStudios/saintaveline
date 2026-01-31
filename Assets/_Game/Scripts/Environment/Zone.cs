using UnityEngine;

public class Zone : MonoBehaviour
{
    [SerializeField] private string _zoneName;
    [SerializeField] private int _priority;

    [SerializeField]
    [TextArea]
    private string _zoneDescription;

    public string ZoneName => _zoneName;
    public string ZoneDescription => _zoneDescription;
    public int Priority => _priority;

    private void OnValidate()
    {
        // AI: Ensure the BoxCollider is always configured as a trigger.
        var box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
        }
    }
}
