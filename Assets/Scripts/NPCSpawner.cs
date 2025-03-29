using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform[] spawnPoints;
    public int npcCount = 3;

    void Start()
    {
        SpawnNPCs();
    }

    void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform spawn = spawnPoints[index];
            
            // Find closest NavMesh-valid position near spawn point
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawn.position, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Spawn NPC at this valid NavMesh location
                Instantiate(npcPrefab, navHit.position, spawn.rotation);
            }
            else
            {
                Debug.LogWarning($"No NavMesh position found near spawn point: {spawn.name}");
            }
        }
    }
}
