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
    private bool protegoShieldActive = false; // New field for Protego shield

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

        // Don't override prefab settings for radius and height - use what's configured in prefab
        // Only set stopping distance to a small value to avoid conflicts with our stopRadius logic
        agent.stoppingDistance = 0.1f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 50;

        wobbleSeed = Random.value * 1000f;
    }

    void Update()
    {
        // Check if player exists, agent exists, agent is enabled, and agent is on NavMesh
        if (player == null || agent == null || !agent.isOnNavMesh) return;

        // If Protego shield is active, stop all movement
        if (protegoShieldActive)
        {
            agent.isStopped = true;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // If we're within stop radius, stop the agent
        if (distanceToPlayer <= stopRadius)
        {
            agent.isStopped = true;
            return;
        }
        
        // If we're outside stop radius, resume movement
        agent.isStopped = false;

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

    // New method to handle Protego shield
    public void SetProtegoShieldActive(bool active)
    {
        protegoShieldActive = active;
        
        if (active)
        {
            Debug.Log($"[EnemyMovement] {gameObject.name} movement stopped by Protego shield");
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
        else
        {
            Debug.Log($"[EnemyMovement] {gameObject.name} movement resumed after Protego shield");
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }

    // Public getter for shield status
    public bool IsProtegoShieldActive() => protegoShieldActive;
} 