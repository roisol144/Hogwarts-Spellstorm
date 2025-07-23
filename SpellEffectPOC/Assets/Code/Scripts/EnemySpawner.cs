using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class EnemyPortalConfig
{
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;
    
    [Tooltip("Portal prefab to use for this enemy")]
    public GameObject portalPrefab;
    
    [Tooltip("Height offset for this enemy above ground")]
    public float heightOffset = 1f;
    
    [Header("Dementor Special Settings")]
    [Tooltip("Enable random height for dementors - randomize between min and max")]
    public bool useRandomHeight = false;
    
    [Tooltip("Minimum height offset for random height (dementors)")]
    public float minHeightOffset = 1f;
    
    [Tooltip("Maximum height offset for random height (dementors)")]
    public float maxHeightOffset = 5f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy-Portal Configuration")]
    [Tooltip("Configure enemy-portal pairs with individual settings")]
    public List<EnemyPortalConfig> enemyPortalConfigs = new List<EnemyPortalConfig>();
    
    [Header("Spawn Area Configuration")]
    [Tooltip("Center point for spawn area")]
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    
    [Tooltip("Radius of spawn area")]
    [SerializeField] private float spawnRadius = 50f;
    
    [Tooltip("Maximum attempts to find valid spawn location")]
    [SerializeField] private int maxSpawnAttempts = 50;
    
    [Tooltip("NavMesh sample distance for finding valid positions")]
    [SerializeField] private float navMeshSampleDistance = 10f;
    
    [Tooltip("Minimum distance from player to spawn enemies")]
    [SerializeField] private float minDistanceFromPlayer = 10f;
    
    [Header("Timing")]
    public float spawnInterval = 5f;
    
    [Header("Portal Effects")]
    [Tooltip("How long the portal should stay active before being destroyed")]
    public float portalDuration = 3f;
    
    [Tooltip("Delay between portal spawn and enemy spawn")]
    public float enemySpawnDelay = 1f;
    
    [Tooltip("Duration for enemy emergence animation")]
    public float enemyEmergenceDuration = 1f;
    
    [Header("Spawning Mode")]
    [Tooltip("When true: spawn enemies every spawnInterval seconds. When false: only spawn one enemy at a time")]
    public bool useTimerBasedSpawning = true;

    // Private variables
    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private Transform playerTransform;

    void Start()
    {
        // Find player reference
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            playerTransform = mainCamera.transform;
        }
        
        // Validate configuration
        if (enemyPortalConfigs.Count == 0)
        {
            Debug.LogError("[EnemySpawner] No enemy-portal configurations set! Please configure at least one enemy-portal pair in the inspector.");
            return;
        }
        
        Debug.Log($"[EnemySpawner] System initialized with {enemyPortalConfigs.Count} enemy types. Spawn center: {spawnCenter}, radius: {spawnRadius}");
    }

    void Update()
    {
        if (enemyPortalConfigs.Count == 0) return;
        
        if (useTimerBasedSpawning)
        {
            // Timer-based spawning
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                StartCoroutine(SpawnRandomEnemyWithPortal());
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
                StartCoroutine(SpawnRandomEnemyWithPortal());
            }
        }
    }

    /// <summary>
    /// Spawns a random enemy at a random NavMesh location with portal effect
    /// </summary>
    private IEnumerator SpawnRandomEnemyWithPortal()
    {
        // Set spawning flag for single enemy mode
        if (!useTimerBasedSpawning)
        {
            isSpawning = true;
        }

        // Pick a random enemy-portal configuration
        EnemyPortalConfig config = enemyPortalConfigs[Random.Range(0, enemyPortalConfigs.Count)];
        
        // Validate the configuration
        if (config.enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Invalid configuration - missing enemy prefab");
            if (!useTimerBasedSpawning) isSpawning = false;
            yield break;
        }

        // Find a random valid spawn position
        Vector3 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("[EnemySpawner] Could not find valid spawn position");
            if (!useTimerBasedSpawning) isSpawning = false;
            yield break;
        }

        // Calculate height offset (with special handling for dementors)
        float heightOffset = config.heightOffset;
        if (config.useRandomHeight)
        {
            heightOffset = Random.Range(config.minHeightOffset, config.maxHeightOffset);
            Debug.Log($"[EnemySpawner] Using random height offset for {config.enemyPrefab.name}: {heightOffset}");
        }

        // Calculate portal position (above spawn point with height offset)
        Vector3 portalPosition = spawnPosition + Vector3.up * (heightOffset + 2f); // Extra 2 units for portal
        
        // Calculate portal rotation to face the player
        Quaternion portalRotation = CalculatePortalRotation(portalPosition);
        
        Debug.Log($"[EnemySpawner] Spawning {config.enemyPrefab.name} at random position: {spawnPosition} with portal at: {portalPosition}");
        
        // Spawn portal effect if available
        GameObject portal = null;
        if (config.portalPrefab != null)
        {
            portal = Instantiate(config.portalPrefab, portalPosition, portalRotation);
            Debug.Log($"[EnemySpawner] Spawned portal {portal.name} at position {portal.transform.position}");
            
            // Destroy portal after duration
            Destroy(portal, portalDuration);
        }

        // Wait for the enemy spawn delay
        yield return new WaitForSeconds(enemySpawnDelay);

        // Calculate final enemy position with height offset
        Vector3 enemySpawnPosition = spawnPosition + Vector3.up * heightOffset;

        // Spawn the enemy at portal height first (for emergence effect)
        Debug.Log($"[EnemySpawner] Spawning {config.enemyPrefab.name} at height: {enemySpawnPosition}");
        GameObject spawned = Instantiate(config.enemyPrefab, portalPosition, portalRotation);
        Debug.Log($"[EnemySpawner] Spawned {spawned.name} at position {spawned.transform.position}");

        // Add enemy emergence animation to the final ground position
        StartCoroutine(AnimateEnemyEmergence(spawned, portalPosition, enemySpawnPosition));

        // Add to active enemies list if using single enemy mode
        if (!useTimerBasedSpawning)
        {
            activeEnemies.Add(spawned);
            Debug.Log($"[EnemySpawner] Added enemy to tracking list. Active enemies: {activeEnemies.Count}");
            isSpawning = false; // Reset spawning flag
        }
    }

    /// <summary>
    /// Finds a valid spawn position on the NavMesh away from the player
    /// </summary>
    private Vector3 FindValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Generate random position within spawn radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Try to find valid NavMesh position (exclude Not Walkable areas)
            NavMeshHit hit;
            int walkableAreaMask = 1 << 0; // Only include Walkable area (Area 0)
            Debug.Log($"[EnemySpawner] Testing position {randomPosition} with area mask {walkableAreaMask}");
            if (NavMesh.SamplePosition(randomPosition, out hit, navMeshSampleDistance, walkableAreaMask))
            {
                Vector3 navMeshPosition = hit.position;
                
                // Check distance from player
                if (playerTransform != null)
                {
                    float distanceFromPlayer = Vector3.Distance(navMeshPosition, playerTransform.position);
                    if (distanceFromPlayer < minDistanceFromPlayer)
                    {
                        continue; // Too close to player, try again
                    }
                }
                
                Debug.Log($"[EnemySpawner] Found valid spawn position: {navMeshPosition} (attempt {attempt + 1})");
                return navMeshPosition;
            }
        }
        
        Debug.LogWarning($"[EnemySpawner] Failed to find valid spawn position after {maxSpawnAttempts} attempts.");
        return Vector3.zero;
    }

    /// <summary>
    /// Calculates the rotation for the portal to face towards the player
    /// </summary>
    private Quaternion CalculatePortalRotation(Vector3 portalPosition)
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - portalPosition).normalized;
            
            // Make the portal face the player (portal "looks at" the player)
            // We only care about horizontal rotation (Y-axis), ignore vertical differences
            directionToPlayer.y = 0;
            
            if (directionToPlayer.magnitude > 0.01f) // Make sure we have a valid direction
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                Debug.Log($"[EnemySpawner] Portal facing player at direction: {directionToPlayer}");
                return lookRotation;
            }
        }
        
        // Fallback to default rotation if player not found
        Debug.Log("[EnemySpawner] Player not found, using default rotation for portal");
        return Quaternion.identity;
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

    // Public methods for external control
    public void ForceSpawnEnemy()
    {
        StartCoroutine(SpawnRandomEnemyWithPortal());
    }

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

    public void ClearActiveEnemies()
    {
        activeEnemies.Clear();
        isSpawning = false;
        Debug.Log("[EnemySpawner] Cleared active enemies list");
    }

    // Gizmos for visualizing spawn area
    void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        
        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnCenter, 1f);
        
        // Draw minimum distance from player
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
        }
    }
}