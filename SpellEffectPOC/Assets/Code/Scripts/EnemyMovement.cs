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
        agent.stoppingDistance = stopRadius;

        wobbleSeed = Random.value * 1000f;
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopRadius)
        {
            // Calculate direction to player
            Vector3 toPlayer = (player.position - transform.position).normalized;

            // Calculate a smooth wobble offset perpendicular to the direction to the player
            float time = Time.time * wobbleSpeed + wobbleSeed;
            Vector3 perp = Vector3.Cross(toPlayer, Vector3.up).normalized;
            float wobble = Mathf.PerlinNoise(time, 0f) * 2f - 1f; // Range [-1, 1]
            Vector3 offset = perp * wobble * wobbleAmount;

            // Final target is always in the general direction of the player, with a little drift
            Vector3 target = player.position - toPlayer * stopRadius + offset;

            Debug.Log($"{gameObject.name} targeting {target} (player at {player.position})");

            agent.isStopped = false;
            agent.SetDestination(target);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }
} 