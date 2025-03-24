using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainEdgeDetector : MonoBehaviour
{
    public Terrain terrain;
    public float edgeDistanceThreshold = 5f; // Distance from edge

    void Update()
    {
        if (terrain == null) return;

        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 playerPosition = transform.position;

        // Calculate distance from each edge
        float distanceToLeftEdge = playerPosition.x;
        float distanceToRightEdge = terrainSize.x - playerPosition.x;
        float distanceToTopEdge = terrainSize.z - playerPosition.z;
        float distanceToBottomEdge = playerPosition.z;

        if (distanceToLeftEdge <= edgeDistanceThreshold ||
            distanceToRightEdge <= edgeDistanceThreshold ||
            distanceToTopEdge <= edgeDistanceThreshold ||
            distanceToBottomEdge <= edgeDistanceThreshold)
        {
            gameOver();
        }
    }

    private void gameOver()
    {
        Debug.Log("Game Over!");
        SceneManager.LoadScene("MainMenu");
    }
}
