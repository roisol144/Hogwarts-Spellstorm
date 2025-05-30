using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public List<GameObject> enemyPrefabs; // Assign all enemy prefabs here
    public List<Transform> spawnPoints;   // Assign all spawn points here
    public float spawnInterval = 5f;      // Seconds between spawns

    [Header("Spawning Mode")]
    [Tooltip("When true: spawn enemies every spawnInterval seconds. When false: only spawn one enemy at a time, new one spawns when current dies")]
    public bool useTimerBasedSpawning = true;

    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies

    void Update()
    {
        if (useTimerBasedSpawning)
        {
            // Original timer-based spawning
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }
        else
        {
            // Single enemy spawning mode
            // Clean up null references from destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            // If no enemies are alive, spawn a new one
            if (activeEnemies.Count == 0)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Count == 0) return;

        Debug.Log("Trying to spawn: " + (enemyPrefabs.Count > 0 ? enemyPrefabs[0].name : "none") + " at " + (spawnPoints.Count > 0 ? spawnPoints[0].position.ToString() : "none"));

        // Pick a random enemy and spawn point
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        Debug.Log("Spawning enemy at: " + spawnPoint.position);
        GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Spawned {spawned.name} at position {spawned.transform.position}");

        // Add to active enemies list if using single enemy mode
        if (!useTimerBasedSpawning)
        {
            activeEnemies.Add(spawned);
            Debug.Log($"[EnemySpawner] Added enemy to tracking list. Active enemies: {activeEnemies.Count}");
        }
    }

    // Public method to manually spawn an enemy (for external calls)
    public void ForceSpawnEnemy()
    {
        SpawnEnemy();
    }

    // Public method to get the count of active enemies (useful for debugging)
    public int GetActiveEnemyCount()
    {
        if (!useTimerBasedSpawning)
        {
            // Clean up null references first
            activeEnemies.RemoveAll(enemy => enemy == null);
            return activeEnemies.Count;
        }
        else
        {
            // In timer mode, count all enemies in scene
            return FindObjectsOfType<Enemy>().Length;
        }
    }

    // Public method to clear all active enemies (useful for resetting)
    public void ClearActiveEnemies()
    {
        activeEnemies.Clear();
        Debug.Log("[EnemySpawner] Cleared active enemies list");
    }
}