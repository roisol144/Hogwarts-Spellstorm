using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public float stopRadius = 2f;
    public float wobbleAmount = 1f; // How far to drift from the direct path
    public float wobbleSpeed = 0.5f; // How fast the wobble changes

    private Transform player;
    private NavMeshAgent agent;
    private float wobbleSeed;

    void Start()
    {
        // Use the main camera as the player reference
        player = Camera.main.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent found on " + gameObject.name);
            return;
        }

        // Basic NavMeshAgent setup
        agent.stoppingDistance = stopRadius;
        agent.radius = 0.5f;
        agent.height = 2f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 50;

        wobbleSeed = Random.value * 1000f;
    }

    void Update()
    {
        if (player == null || agent == null) return;

        Vector3 toPlayer = (player.position - transform.position).normalized;
        float time = Time.time * wobbleSpeed + wobbleSeed;
        Vector3 perp = Vector3.Cross(toPlayer, Vector3.up).normalized;
        float wobble = Mathf.PerlinNoise(time, 0f) * 2f - 1f;
        Vector3 offset = perp * wobble * wobbleAmount;
        Vector3 target = player.position - toPlayer * stopRadius + offset;

        // Project the target onto the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find a valid NavMesh position near target!");
        }

    }

    void OnDrawGizmos()
    {
        if (agent != null)
        {
            // Draw the agent's radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.radius);
            
            // Draw the stopping distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopRadius);
        }
    }
} 