using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public List<GameObject> enemyPrefabs; // Assign all enemy prefabs here
    public List<Transform> spawnPoints;   // Assign all spawn points here
    public float spawnInterval = 5f;      // Seconds between spawns

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
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
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}