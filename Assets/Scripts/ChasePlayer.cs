using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    public float detectionRange = 50f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Optional: stop moving if player is out of range
            agent.ResetPath();
        }
    }
}
