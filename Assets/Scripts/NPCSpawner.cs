using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform[] spawnPoints;
    public int npcCount = 3;
    public Color npcColor = Color.red;
    void Start()
    {
        SpawnNPCs();
    }

    void SpawnNPCs()
    {
        HashSet<Vector3Int> usedSpawnPoints = new HashSet<Vector3Int>();

        for (int i = 0; i < npcCount; i++)
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform spawn = spawnPoints[index];
            
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawn.position, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                GameObject npc = Instantiate(npcPrefab, navHit.position, spawn.rotation);
                npc.GetComponent<Renderer>().material.color = npcColor;

                bool doneLocating = false;
                while (!doneLocating)
                {
                    if (!usedSpawnPoints.Contains(Vector3Int.RoundToInt(npc.transform.position)))
                    {
                        usedSpawnPoints.Add(Vector3Int.RoundToInt(npc.transform.position));
                        doneLocating = true; 
                        continue;
                    }

                    Vector3 newPosition = npc.transform.position;
                    newPosition.x += Random.Range(-2f, 2f);
                    newPosition.z += Random.Range(-2f, 2f);
                    npc.transform.position = newPosition;
                }
            }
            else
            {
                Debug.LogWarning($"No NavMesh position found near spawn point: {spawn.name}");
            }
        }
    }
}
