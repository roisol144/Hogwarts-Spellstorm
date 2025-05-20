using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float stopRadius = 2f;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        Debug.Log("EnemyMovement started for: " + gameObject.name);
    }

    void Update()
    {
        Debug.Log(gameObject.name + " is updating, player is " + (player != null ? player.name : "null"));

        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > stopRadius)
        {
            // Move toward player
            Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
            if (move.magnitude > distance - stopRadius)
                move = direction.normalized * (distance - stopRadius);

            transform.position += move;
        }
    }
} 