using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RemoveUnderwaterTrees : MonoBehaviour
{
    public Terrain terrain;
    public float waterLevel = 0.0f; // Set this to your water plane's Y-position, e.g., -2f

    private TreeInstance[] backupTreeInstances;
    private List<TreeInstance> newTreeInstances;

    public enum AllTreeActions
    {
        BackupCurrentTrees,
        RestoreBackupTrees,
        RemoveUnderwaterTrees
    }

    public AllTreeActions performAction;

    [ContextMenu("Modify Tree Data")]
    void ModifyTreeData()
    {
        if (!terrain)
        {
            terrain = Terrain.activeTerrain;
        }

        switch (performAction)
        {
            case AllTreeActions.BackupCurrentTrees:
                backupTreeInstances = terrain.terrainData.treeInstances;
                Debug.Log("Current trees have been stored in backup data");
                break;

            case AllTreeActions.RestoreBackupTrees:
                if (backupTreeInstances != null)
                {
                    terrain.terrainData.treeInstances = backupTreeInstances;
                    Debug.Log("Trees have been restored from the backup data");
                }
                else
                {
                    Debug.Log("NO backup data FOUND ....");
                }
                break;

            case AllTreeActions.RemoveUnderwaterTrees:
                Debug.Log("Removing trees below water level ....");

                Vector3 terrainSize = terrain.terrainData.size;
                Vector3 terrainPos = terrain.transform.position;
                TreeInstance[] treeInstances = terrain.terrainData.treeInstances;
                Debug.Log("Old : Total Trees = " + treeInstances.Length);

                newTreeInstances = new List<TreeInstance>();

                for (int t = 0; t < treeInstances.Length; t++)
                {
                    float treeWorldY = treeInstances[t].position.y * terrainSize.y + terrainPos.y;
                    if (treeWorldY > waterLevel)
                    {
                        newTreeInstances.Add(treeInstances[t]);
                    }
                }

                terrain.terrainData.treeInstances = newTreeInstances.ToArray();
                Debug.Log("New : Total Trees = " + terrain.terrainData.treeInstances.Length);
                break;
        }
    }
}