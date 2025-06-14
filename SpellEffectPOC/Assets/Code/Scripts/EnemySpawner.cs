using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class SpawnPointConfig
{
    [Tooltip("The transform where enemies will spawn")]
    public Transform spawnPoint;
    
    [Tooltip("List of enemy prefabs that can spawn at this point")]
    public List<GameObject> enemyPrefabs;
    
    [Tooltip("Optional: Leave empty to use global portal, or assign specific portal for this spawn point")]
    public GameObject customPortalPrefab;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [Tooltip("Configure each spawn point with its specific enemy types")]
    public List<SpawnPointConfig> spawnPointConfigs;
    
    [Header("Legacy Support (Optional)")]
    [Tooltip("Legacy: Global enemy prefabs list - only used if spawnPointConfigs is empty")]
    public List<GameObject> enemyPrefabs; // Keep for backward compatibility
    [Tooltip("Legacy: Global spawn points list - only used if spawnPointConfigs is empty")]
    public List<Transform> spawnPoints;   // Keep for backward compatibility
    
    [Header("Timing")]
    public float spawnInterval = 5f;      // Seconds between spawns

    [Header("Portal Effects")]
    [Tooltip("Default portal prefab - used when spawn point doesn't have custom portal")]
    public GameObject portalPrefab; // Assign one of the portal prefabs from Hovl Studio/Magic effects pack/Prefabs/Portals
    
    [Tooltip("How long the portal should stay active before being destroyed")]
    public float portalDuration = 3f;
    
    [Tooltip("Delay between portal spawn and enemy spawn (to simulate enemy coming through portal)")]
    public float enemySpawnDelay = 1f;
    
    [Tooltip("Height offset for portal above spawn point")]
    public float portalHeightOffset = 2f;
    
    [Tooltip("Duration for enemy emergence animation")]
    public float enemyEmergenceDuration = 1f;

    [Header("Spawning Mode")]
    [Tooltip("When true: spawn enemies every spawnInterval seconds. When false: only spawn one enemy at a time, new one spawns when current dies")]
    public bool useTimerBasedSpawning = true;

    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies
    private bool isSpawning = false; // Prevent multiple simultaneous spawns in single enemy mode

    void Update()
    {
        if (useTimerBasedSpawning)
        {
            // Original timer-based spawning
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                StartCoroutine(SpawnEnemyWithPortal());
                timer = 0f;
            }
        }
        else
        {
            // Single enemy spawning mode
            // Clean up null references from destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            // If no enemies are alive and not currently spawning, spawn a new one
            if (activeEnemies.Count == 0 && !isSpawning)
            {
                StartCoroutine(SpawnEnemyWithPortal());
            }
        }
    }

    /// <summary>
    /// Spawns a portal effect followed by an enemy after a delay
    /// </summary>
    private IEnumerator SpawnEnemyWithPortal()
    {
        // Check if we have configured spawn points
        if (spawnPointConfigs.Count > 0)
        {
            // Use new spawn point configuration system
            yield return StartCoroutine(SpawnEnemyFromConfig());
        }
        else if (enemyPrefabs.Count > 0 && spawnPoints.Count > 0)
        {
            // Fall back to legacy system
            yield return StartCoroutine(SpawnEnemyLegacy());
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] No spawn points configured! Please set up spawnPointConfigs or legacy lists.");
            yield break;
        }
    }

    /// <summary>
    /// New spawn system using configured spawn points
    /// </summary>
    private IEnumerator SpawnEnemyFromConfig()
    {
        // Set spawning flag for single enemy mode
        if (!useTimerBasedSpawning)
        {
            isSpawning = true;
        }

        // Pick a random spawn point configuration
        SpawnPointConfig config = spawnPointConfigs[Random.Range(0, spawnPointConfigs.Count)];
        
        // Validate the configuration
        if (config.spawnPoint == null || config.enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"[EnemySpawner] Invalid spawn point config - missing spawn point or enemy prefabs");
            if (!useTimerBasedSpawning) isSpawning = false;
            yield break;
        }

        // Pick a random enemy from this spawn point's available enemies
        GameObject prefab = config.enemyPrefabs[Random.Range(0, config.enemyPrefabs.Count)];
        Transform spawnPoint = config.spawnPoint;

        // Use custom portal if available, otherwise use default
        GameObject portalToUse = config.customPortalPrefab != null ? config.customPortalPrefab : portalPrefab;

        // Calculate portal position (higher than spawn point)
        Vector3 portalPosition = spawnPoint.position + Vector3.up * portalHeightOffset;
        
        // Calculate portal rotation to face the player
        Quaternion portalRotation = CalculatePortalRotation(portalPosition, spawnPoint.rotation);
        
        Debug.Log($"[EnemySpawner] Spawning {prefab.name} at {spawnPoint.name} with portal at: {portalPosition}");
        
        // Spawn portal effect if available
        GameObject portal = null;
        if (portalToUse != null)
        {
            portal = Instantiate(portalToUse, portalPosition, portalRotation);
            Debug.Log($"Spawned portal {portal.name} at position {portal.transform.position} facing player");
            
            // Destroy portal after duration
            Destroy(portal, portalDuration);
        }

        // Wait for the enemy spawn delay
        yield return new WaitForSeconds(enemySpawnDelay);

        // Find the actual ground level beneath the portal using raycast
        Vector3 groundPosition = FindGroundPosition(portalPosition, spawnPoint.position);

        // Spawn the enemy at portal height first (for emergence effect)
        Debug.Log($"[EnemySpawner] Spawning {prefab.name} at portal height: {portalPosition}");
        GameObject spawned = Instantiate(prefab, portalPosition, portalRotation);
        Debug.Log($"Spawned {spawned.name} at position {spawned.transform.position}");

        // Add enemy emergence animation to the actual ground
        StartCoroutine(AnimateEnemyEmergence(spawned, portalPosition, groundPosition));

        // Add to active enemies list if using single enemy mode
        if (!useTimerBasedSpawning)
        {
            activeEnemies.Add(spawned);
            Debug.Log($"[EnemySpawner] Added enemy to tracking list. Active enemies: {activeEnemies.Count}");
            isSpawning = false; // Reset spawning flag
        }
    }

    /// <summary>
    /// Legacy spawn system for backward compatibility
    /// </summary>
    private IEnumerator SpawnEnemyLegacy()
    {
        // Set spawning flag for single enemy mode
        if (!useTimerBasedSpawning)
        {
            isSpawning = true;
        }

        // Pick a random enemy and spawn point
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Calculate portal position (higher than spawn point)
        Vector3 portalPosition = spawnPoint.position + Vector3.up * portalHeightOffset;
        
        // Calculate portal rotation to face the player
        Quaternion portalRotation = CalculatePortalRotation(portalPosition, spawnPoint.rotation);
        
        Debug.Log("Spawning portal at: " + portalPosition);
        
        // Spawn portal effect if available
        GameObject portal = null;
        if (portalPrefab != null)
        {
            portal = Instantiate(portalPrefab, portalPosition, portalRotation);
            Debug.Log($"Spawned portal {portal.name} at position {portal.transform.position} facing player");
            
            // Destroy portal after duration
            Destroy(portal, portalDuration);
        }

        // Wait for the enemy spawn delay
        yield return new WaitForSeconds(enemySpawnDelay);

        // Find the actual ground level beneath the portal using raycast
        Vector3 groundPosition = FindGroundPosition(portalPosition, spawnPoint.position);

        // Spawn the enemy at portal height first (for emergence effect)
        Debug.Log("Spawning enemy at portal height: " + portalPosition);
        GameObject spawned = Instantiate(prefab, portalPosition, portalRotation);
        Debug.Log($"Spawned {spawned.name} at position {spawned.transform.position}");

        // Add enemy emergence animation to the actual ground
        StartCoroutine(AnimateEnemyEmergence(spawned, portalPosition, groundPosition));

        // Add to active enemies list if using single enemy mode
        if (!useTimerBasedSpawning)
        {
            activeEnemies.Add(spawned);
            Debug.Log($"[EnemySpawner] Added enemy to tracking list. Active enemies: {activeEnemies.Count}");
            isSpawning = false; // Reset spawning flag
        }
    }

    /// <summary>
    /// Calculates the rotation for the portal to face towards the player
    /// </summary>
    private Quaternion CalculatePortalRotation(Vector3 portalPosition, Quaternion fallbackRotation)
    {
        // Try to find the player using the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 playerPosition = mainCamera.transform.position;
            Vector3 directionToPlayer = (playerPosition - portalPosition).normalized;
            
            // Make the portal face the player (portal "looks at" the player)
            // We only care about horizontal rotation (Y-axis), ignore vertical differences
            directionToPlayer.y = 0;
            
            if (directionToPlayer.magnitude > 0.01f) // Make sure we have a valid direction
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                Debug.Log($"Portal facing player at direction: {directionToPlayer}");
                return lookRotation;
            }
        }
        
        // Fallback to spawn point rotation if player not found
        Debug.Log("Player not found, using spawn point rotation for portal");
        return fallbackRotation;
    }

    /// <summary>
    /// Finds the actual ground position beneath the portal using raycast
    /// </summary>
    private Vector3 FindGroundPosition(Vector3 portalPos, Vector3 fallbackPos)
    {
        // Cast a ray downward from the portal position to find the ground
        RaycastHit hit;
        Vector3 rayStart = portalPos;
        Vector3 rayDirection = Vector3.down;
        float maxDistance = portalHeightOffset + 10f; // Look a bit further than expected

        // Use a layermask to hit ground/terrain layers (you can adjust this as needed)
        int layerMask = ~(1 << LayerMask.NameToLayer("Enemy")); // Ignore enemy layer
        
        if (Physics.Raycast(rayStart, rayDirection, out hit, maxDistance, layerMask))
        {
            // Found ground, use that position
            Vector3 groundPos = hit.point;
            Debug.Log($"Found ground at: {groundPos}, hit object: {hit.collider.name}");
            return groundPos;
        }
        else
        {
            // No ground found, use the original spawn point as fallback
            Debug.Log($"No ground found beneath portal, using fallback position: {fallbackPos}");
            return fallbackPos;
        }
    }

    /// <summary>
    /// Animates enemy emerging from portal - moves down and scales up with natural falling motion
    /// </summary>
    private IEnumerator AnimateEnemyEmergence(GameObject enemy, Vector3 startPos, Vector3 endPos)
    {
        if (enemy == null) yield break;

        float elapsedTime = 0f;
        Vector3 originalScale = enemy.transform.localScale;
        
        // Start with smaller scale for dramatic effect
        enemy.transform.localScale = originalScale * 0.1f;

        while (elapsedTime < enemyEmergenceDuration)
        {
            if (enemy == null) yield break; // Enemy might be destroyed during animation

            float t = elapsedTime / enemyEmergenceDuration;
            
            // Use different easing for position and scale
            // For falling: start slow, accelerate (like gravity)
            float fallT = EaseInQuad(t);
            
            // For scaling: smooth and steady growth
            float scaleT = Mathf.SmoothStep(0f, 1f, t);

            // Animate position with gravity-like acceleration
            enemy.transform.position = Vector3.Lerp(startPos, endPos, fallT);
            
            // Animate scale smoothly
            enemy.transform.localScale = Vector3.Lerp(originalScale * 0.1f, originalScale, scaleT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and scale are exact
        if (enemy != null)
        {
            enemy.transform.position = endPos;
            enemy.transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Easing function that starts slow and accelerates (like gravity)
    /// </summary>
    private float EaseInQuad(float t)
    {
        return t * t;
    }

    /// <summary>
    /// Legacy method for backward compatibility - now uses portal system
    /// </summary>
    void SpawnEnemy()
    {
        StartCoroutine(SpawnEnemyWithPortal());
    }

    // Public method to manually spawn an enemy (for external calls)
    public void ForceSpawnEnemy()
    {
        StartCoroutine(SpawnEnemyWithPortal());
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
        isSpawning = false; // Reset spawning flag
        Debug.Log("[EnemySpawner] Cleared active enemies list");
    }
}