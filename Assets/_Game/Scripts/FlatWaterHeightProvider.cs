// AI: FlatWaterHeightProvider.cs - constant flat water level
// AI: Attach to your water root if surface is a plane at fixed Y.

using UnityEngine;

public class FlatWaterHeightProvider : MonoBehaviour, IWaterHeightProvider
{
    [SerializeField] private float _waterLevelY = 0f;

    public bool TryGetHeight(in Vector3 worldPos, out float height, out Vector3 normal)
    {
        height = _waterLevelY;
        normal = Vector3.up;
        return true;
    }
}
 