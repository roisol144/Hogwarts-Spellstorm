using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the collectible challenge system - spawns chocolate frogs or golden eggs
/// every 5 minutes starting at 2 minutes, gives player 2 minutes to collect all items
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Timing Configuration")]
    [Tooltip("Time until first spawn (in seconds) - default 2 minutes")]
    [SerializeField] private float initialDelay = 120f; // 2 minutes
    
    [Tooltip("Interval between spawns (in seconds) - default 5 minutes")]
    [SerializeField] private float spawnInterval = 300f; // 5 minutes
    
    [Tooltip("Time player has to collect all items (in seconds) - default 2 minutes")]
    [SerializeField] private float collectionTimeLimit = 120f; // 2 minutes
    
    [Header("Spawn Configuration")]
    [Tooltip("Number of items to spawn each challenge")]
    [SerializeField] private int itemsPerChallenge = 4;
    
    [Tooltip("Chocolate frog prefab")]
    [SerializeField] private GameObject chocolateFrogPrefab;
    
    [Tooltip("Golden egg prefab")]
    [SerializeField] private GameObject goldenEggPrefab;
    
    [Tooltip("Minimum distance between spawned items")]
    [SerializeField] private float minDistanceBetweenItems = 5f;
    
    [Tooltip("Maximum attempts to find valid spawn location")]
    [SerializeField] private int maxSpawnAttempts = 50;
    
    [Tooltip("NavMesh sample distance for finding valid positions")]
    [SerializeField] private float navMeshSampleDistance = 10f;
    
    [Header("Spawn Bounds")]
    [Tooltip("Center point for spawn area")]
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    
    [Tooltip("Radius of spawn area")]
    [SerializeField] private float spawnRadius = 50f;
    
    [Tooltip("Height offset above ground for spawned items")]
    [SerializeField] private float spawnHeightOffset = 1f;
    
    [Tooltip("Additional height offset for chocolate frog")]
    [SerializeField] private float chocolateFrogHeightOffset = 0f;
    
    [Tooltip("Additional height offset for golden egg")]
    [SerializeField] private float goldenEggHeightOffset = 0f;
    
    [Header("Scoring")]
    [Tooltip("Points awarded for successful collection")]
    [SerializeField] private int successPoints = 200;
    
    [Tooltip("Points penalized for failed collection")]
    [SerializeField] private int failurePoints = 100;
    
    [Header("Audio")]
    [Tooltip("Sound played when challenge starts")]
    [SerializeField] private AudioClip challengeStartSound;
    
    [Tooltip("Sound played when item is collected")]
    [SerializeField] private AudioClip itemCollectedSound;
    
    [Tooltip("Sound played when challenge succeeds")]
    [SerializeField] private AudioClip challengeSuccessSound;
    
    [Tooltip("Sound played when challenge fails")]
    [SerializeField] private AudioClip challengeFailSound;
    
    [Header("UI Messages")]
    [Tooltip("How long challenge announcements stay on screen (seconds)")]
    [SerializeField] private float announcementDuration = 5f;
    
    // Private variables
    private AudioSource audioSource;
    private Camera playerCamera;
    private CollectibleChallenge currentChallenge;
    private bool systemActive = true;
    private Coroutine spawnTimerCoroutine;
    
    // Singleton pattern for easy access
    public static CollectibleSpawner Instance { get; private set; }
    
    // Events
    public System.Action<int, int> OnChallengeStarted; // itemCount, timeLimit
    public System.Action<int, int> OnItemCollected; // collected, total
    public System.Action<bool> OnChallengeCompleted; // success
    public System.Action<float> OnTimerUpdate; // time remaining
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[CollectibleSpawner] Multiple instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Start()
    {
        // Validate prefabs
        if (chocolateFrogPrefab == null || goldenEggPrefab == null)
        {
            Debug.LogError("[CollectibleSpawner] Collectible prefabs not assigned! Please assign chocolate frog and golden egg prefabs.");
            systemActive = false;
            return;
        }
        
        // Setup Timer UI if not already present
        SetupTimerUI();
        
        // Start the spawn timer
        StartSpawnTimer();
        
        Debug.Log($"[CollectibleSpawner] System initialized. First spawn in {initialDelay} seconds, then every {spawnInterval} seconds.");
    }
    
    /// <summary>
    /// Sets up the Timer UI automatically
    /// </summary>
    private void SetupTimerUI()
    {
        // Check if TimerUI already exists
        TimerUI existingTimer = FindObjectOfType<TimerUI>();
        if (existingTimer == null)
        {
            // Create TimerUI GameObject
            GameObject timerGO = new GameObject("TimerUI");
            timerGO.AddComponent<TimerUI>();
            Debug.Log("[CollectibleSpawner] Created TimerUI automatically");
        }
        else
        {
            Debug.Log("[CollectibleSpawner] TimerUI already exists");
        }
    }
    
    void OnDestroy()
    {
        if (spawnTimerCoroutine != null)
        {
            StopCoroutine(spawnTimerCoroutine);
        }
    }
    
    /// <summary>
    /// Starts the main spawn timer system
    /// </summary>
    private void StartSpawnTimer()
    {
        if (!systemActive) return;
        
        if (spawnTimerCoroutine != null)
        {
            StopCoroutine(spawnTimerCoroutine);
        }
        
        spawnTimerCoroutine = StartCoroutine(SpawnTimerCoroutine());
    }
    
    /// <summary>
    /// Main coroutine that handles the 5-minute spawn intervals
    /// </summary>
    private IEnumerator SpawnTimerCoroutine()
    {
        // Wait for initial delay (2 minutes)
        yield return new WaitForSeconds(initialDelay);
        
        while (systemActive)
        {
            // Start a new collectible challenge
            yield return StartCoroutine(StartCollectibleChallenge());
            
            // Wait for the spawn interval (5 minutes)
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// Starts a new collectible challenge
    /// </summary>
    private IEnumerator StartCollectibleChallenge()
    {
        // Don't start new challenge if one is already active
        if (currentChallenge != null && currentChallenge.IsActive)
        {
            Debug.LogWarning("[CollectibleSpawner] Challenge already active, skipping new spawn.");
            yield break;
        }
        
        Debug.Log("[CollectibleSpawner] Starting new collectible challenge!");
        
        // Choose collectible type randomly
        bool spawnChocolatefrogs = Random.value > 0.5f;
        GameObject prefabToSpawn = spawnChocolatefrogs ? chocolateFrogPrefab : goldenEggPrefab;
        string itemType = spawnChocolatefrogs ? "Chocolate Frogs" : "Golden Eggs";
        
        // Find spawn positions
        List<Vector3> spawnPositions = FindSpawnPositions(itemsPerChallenge);
        
        if (spawnPositions.Count < itemsPerChallenge)
        {
            Debug.LogWarning($"[CollectibleSpawner] Could only find {spawnPositions.Count} valid spawn positions out of {itemsPerChallenge} requested.");
        }
        
        // Create challenge object
        GameObject challengeObject = new GameObject($"CollectibleChallenge_{itemType}");
        currentChallenge = challengeObject.AddComponent<CollectibleChallenge>();
        
        // Spawn the items
        List<GameObject> spawnedItems = new List<GameObject>();
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            // Calculate appropriate height offset based on item type
            float totalHeightOffset = spawnHeightOffset;
            if (prefabToSpawn == chocolateFrogPrefab)
            {
                totalHeightOffset += chocolateFrogHeightOffset;
            }
            else if (prefabToSpawn == goldenEggPrefab)
            {
                totalHeightOffset += goldenEggHeightOffset;
            }
            
            // Apply height offset to prevent items from sinking into ground
            Vector3 spawnPosition = spawnPositions[i] + Vector3.up * totalHeightOffset;
            GameObject item = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            // Add collectible component if not present
            Collectible collectible = item.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = item.AddComponent<Collectible>();
            }
            
            spawnedItems.Add(item);
        }
        
        // Initialize the challenge
        currentChallenge.Initialize(spawnedItems, collectionTimeLimit, successPoints, failurePoints);
        currentChallenge.OnChallengeCompleted += OnChallengeCompletedHandler;
        currentChallenge.OnItemCollected += OnItemCollectedHandler;
        currentChallenge.OnTimerUpdate += OnTimerUpdateHandler;
        
        // Play start sound
        if (challengeStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(challengeStartSound);
        }
        
        // Notify UI
        OnChallengeStarted?.Invoke(spawnedItems.Count, (int)collectionTimeLimit);
        
        // Show challenge message
        string message = $"Collectible Challenge Started! Collect all {spawnedItems.Count} {itemType} in {collectionTimeLimit / 60f:F1} minutes!";
        MagicalDebugUI.ShowHint(message, announcementDuration);
        
        Debug.Log($"[CollectibleSpawner] Spawned {spawnedItems.Count} {itemType} at various locations. Challenge started!");
    }
    
    /// <summary>
    /// Finds valid spawn positions on the NavMesh
    /// </summary>
    private List<Vector3> FindSpawnPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        
        for (int i = 0; i < count; i++)
        {
            Vector3 position = FindValidSpawnPosition(positions);
            if (position != Vector3.zero)
            {
                positions.Add(position);
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// Finds a single valid spawn position that's not too close to existing positions
    /// </summary>
    private Vector3 FindValidSpawnPosition(List<Vector3> existingPositions)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Generate random position within spawn radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Try to find valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                Vector3 navMeshPosition = hit.position;
                
                // Check distance from existing positions
                bool validDistance = true;
                foreach (Vector3 existingPos in existingPositions)
                {
                    if (Vector3.Distance(navMeshPosition, existingPos) < minDistanceBetweenItems)
                    {
                        validDistance = false;
                        break;
                    }
                }
                
                if (validDistance)
                {
                    return navMeshPosition;
                }
            }
        }
        
        Debug.LogWarning("[CollectibleSpawner] Failed to find valid spawn position after maximum attempts.");
        return Vector3.zero;
    }
    
    /// <summary>
    /// Event handler for timer updates
    /// </summary>
    private void OnTimerUpdateHandler(float timeRemaining)
    {
        // Forward timer update to UI
        OnTimerUpdate?.Invoke(timeRemaining);
    }
    
    /// <summary>
    /// Event handler for when an item is collected
    /// </summary>
    private void OnItemCollectedHandler(int collected, int total)
    {
        // Play collection sound
        if (itemCollectedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(itemCollectedSound);
        }
        
        // Show progress message
        string message = $"Collectible {collected}/{total} found! Keep searching!";
        MagicalDebugUI.ShowHint(message, 2f); // Shorter duration for progress messages
        
        Debug.Log($"[CollectibleSpawner] Item collected: {collected}/{total}");
        
        // Forward event
        OnItemCollected?.Invoke(collected, total);
    }
    
    /// <summary>
    /// Event handler for when a challenge is completed (success or failure)
    /// </summary>
    private void OnChallengeCompletedHandler(bool success, int itemsCollected, int totalItems)
    {
        if (success)
        {
            // Success!
            ScoreManager.NotifyScore(successPoints);
            
            if (challengeSuccessSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(challengeSuccessSound);
            }
            
            string message = $"Challenge Complete! All {totalItems} items collected! +{successPoints} points!";
            MagicalDebugUI.ShowHint(message, announcementDuration);
            
            Debug.Log($"[CollectibleSpawner] Challenge succeeded! Player collected all {totalItems} items. Awarded {successPoints} points.");
        }
        else
        {
            // Failure
            ScoreManager.NotifyPenalty(failurePoints, "Failed to collect all items in time");
            
            if (challengeFailSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(challengeFailSound);
            }
            
            string message = $"Challenge Failed! Only {itemsCollected}/{totalItems} items collected. -{failurePoints} points.";
            MagicalDebugUI.ShowHint(message, announcementDuration);
            
            Debug.Log($"[CollectibleSpawner] Challenge failed! Player only collected {itemsCollected}/{totalItems} items. Applied {failurePoints} point penalty.");
        }
        
        // Forward event
        OnChallengeCompleted?.Invoke(success);
        
        // Clean up
        currentChallenge = null;
    }
    
    /// <summary>
    /// Stops the spawn system
    /// </summary>
    public void StopSystem()
    {
        systemActive = false;
        
        if (spawnTimerCoroutine != null)
        {
            StopCoroutine(spawnTimerCoroutine);
            spawnTimerCoroutine = null;
        }
        
        if (currentChallenge != null)
        {
            currentChallenge.ForceEndChallenge();
        }
        
        Debug.Log("[CollectibleSpawner] System stopped.");
    }
    
    /// <summary>
    /// Restarts the spawn system
    /// </summary>
    public void RestartSystem()
    {
        systemActive = true;
        StartSpawnTimer();
        Debug.Log("[CollectibleSpawner] System restarted.");
    }
    
    /// <summary>
    /// Manually triggers a collectible challenge (for testing)
    /// </summary>
    [ContextMenu("Trigger Challenge Now")]
    public void TriggerChallengeNow()
    {
        if (!systemActive)
        {
            Debug.LogWarning("[CollectibleSpawner] System is not active. Cannot trigger challenge.");
            return;
        }
        
        StartCoroutine(StartCollectibleChallenge());
    }
    
    void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        
        // Draw center point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnCenter, 1f);
    }
} 