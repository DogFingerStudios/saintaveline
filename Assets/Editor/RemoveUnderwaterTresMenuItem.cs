using UnityEditor;
using UnityEngine;

public class RemoveUnderwaterTreesMenuItem
{
    [MenuItem("Terrain/Remove Underwater Trees")]
    static void NewRemoveUnderwaterTrees()
    {
        GameObject go = new GameObject("RemoveUnderwaterTrees");
        go.transform.position = Vector3.zero;
        go.AddComponent<RemoveUnderwaterTrees>();
    }
}