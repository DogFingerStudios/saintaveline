#nullable enable
using UnityEngine;

public class Zone : MonoBehaviour
{
    [SerializeField] private ZoneData _data = null!;
    [SerializeField] private int _priority;

    public ZoneData Data => _data;
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
