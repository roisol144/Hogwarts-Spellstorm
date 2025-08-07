using UnityEngine;
using System;

/// <summary>
/// Manages game difficulty levels and win conditions
/// </summary>
public class GameLevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelConfig
    {
        [Header("Level Settings")]
        public string levelName;
        public int winScore;
        
        [Header("Enemy Spawn Settings")]
        [Tooltip("Time between enemy spawns in seconds")]
        public float enemySpawnInterval;
        
        [Header("Collectible Settings")]
        [Tooltip("Time limit for collectible challenges in seconds")]
        public float collectibleTimeLimit;
        
        public LevelConfig(string name, int score, float enemyInterval, float collectibleTime)
        {
            levelName = name;
            winScore = score;
            enemySpawnInterval = enemyInterval;
            collectibleTimeLimit = collectibleTime;
        }
    }
    
    [Header("Level Selection")]
    [SerializeField] private int currentLevelIndex = 0; // Default to Beginner (0)
    
    [Header("Level Configurations")]
    [SerializeField] private LevelConfig[] levels = new LevelConfig[]
    {
        new LevelConfig("Beginner", 500, 10f, 180f),    // Level 1: 500 points, 10s enemy spawn, 3min collectible
        new LevelConfig("Intermediate", 800, 8f, 120f), // Level 2: 800 points, 8s enemy spawn, 2min collectible  
        new LevelConfig("Advanced", 1000, 5f, 90f)      // Level 3: 1000 points, 5s enemy spawn, 1.5min collectible
    };
    
    [Header("Audio")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioSource audioSource;
    
    // Singleton pattern
    public static GameLevelManager Instance { get; private set; }
    
    // Events
    public static event Action<LevelConfig> OnLevelChanged;
    public static event Action<int> OnVictoryAchieved; // Pass final score
    
    // Properties
    public LevelConfig CurrentLevel => levels[currentLevelIndex];
    public int CurrentLevelIndex => currentLevelIndex;
    public string CurrentLevelName => CurrentLevel.levelName;
    public int CurrentWinScore => CurrentLevel.winScore;
    
    private bool hasWon = false;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Validate level configurations
        ValidateLevelConfigs();
    }
    
    void Start()
    {
        // Subscribe to score changes to check for win condition
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += CheckWinCondition;
        }
        
        // Apply current level settings
        ApplyCurrentLevel();
        
        // Play defend castle announcement when game begins
        GameAnnouncementAudio.PlayDefendTheCastleAnnouncement();
        
        Debug.Log($"[GameLevelManager] Game started on {CurrentLevelName} level. Win condition: {CurrentWinScore} points");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= CheckWinCondition;
        }
    }
    
    private void ValidateLevelConfigs()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("[GameLevelManager] No level configurations found! Creating default levels.");
            levels = new LevelConfig[]
            {
                new LevelConfig("Beginner", 500, 10f, 180f),
                new LevelConfig("Intermediate", 800, 8f, 120f),
                new LevelConfig("Advanced", 1000, 5f, 90f)
            };
        }
        
        // Clamp current level index
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levels.Length - 1);
    }
    
    /// <summary>
    /// Sets the current game level
    /// </summary>
    public void SetLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"[GameLevelManager] Invalid level index: {levelIndex}. Must be between 0 and {levels.Length - 1}");
            return;
        }
        
        currentLevelIndex = levelIndex;
        hasWon = false; // Reset win state when changing levels
        
        ApplyCurrentLevel();
        
        Debug.Log($"[GameLevelManager] Level changed to: {CurrentLevelName} (Win Score: {CurrentWinScore})");
    }
    
    /// <summary>
    /// Sets level by name
    /// </summary>
    public void SetLevel(string levelName)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].levelName.Equals(levelName, StringComparison.OrdinalIgnoreCase))
            {
                SetLevel(i);
                return;
            }
        }
        
        Debug.LogError($"[GameLevelManager] Level '{levelName}' not found!");
    }
    
    /// <summary>
    /// Applies the current level settings to game systems
    /// </summary>
    private void ApplyCurrentLevel()
    {
        LevelConfig level = CurrentLevel;
        
        // Update enemy spawner
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.spawnInterval = level.enemySpawnInterval;
            Debug.Log($"[GameLevelManager] Updated enemy spawn interval to {level.enemySpawnInterval}s");
        }
        else
        {
            Debug.LogWarning("[GameLevelManager] EnemySpawner not found! Enemy spawn interval not updated.");
        }
        
        // Update collectible spawner
        CollectibleSpawner collectibleSpawner = CollectibleSpawner.Instance;
        if (collectibleSpawner != null)
        {
            // We'll need to add a method to update the collection time limit
            UpdateCollectibleSpawner(collectibleSpawner, level.collectibleTimeLimit);
            Debug.Log($"[GameLevelManager] Updated collectible time limit to {level.collectibleTimeLimit}s");
        }
        else
        {
            Debug.LogWarning("[GameLevelManager] CollectibleSpawner not found! Collectible time limit not updated.");
        }
        
        // Notify listeners
        OnLevelChanged?.Invoke(level);
    }
    
    /// <summary>
    /// Updates collectible spawner settings
    /// </summary>
    private void UpdateCollectibleSpawner(CollectibleSpawner spawner, float newTimeLimit)
    {
        spawner.UpdateCollectionTimeLimit(newTimeLimit);
    }
    
    /// <summary>
    /// Checks if the player has achieved the win condition
    /// </summary>
    private void CheckWinCondition(int currentScore)
    {
        if (hasWon) return; // Already won, don't trigger again
        
        if (currentScore >= CurrentWinScore)
        {
            hasWon = true;
            TriggerVictory(currentScore);
        }
    }
    
    /// <summary>
    /// Triggers victory sequence
    /// </summary>
    private void TriggerVictory(int finalScore)
    {
        Debug.Log($"[GameLevelManager] VICTORY! Player achieved {finalScore} points on {CurrentLevelName} level!");
        
        // Play victory sound
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // Play Hogwarts is safe announcement
        GameAnnouncementAudio.PlayHogwartsIsSafeAnnouncement();
        
        // Notify listeners (GameOverUI will handle the victory screen)
        OnVictoryAchieved?.Invoke(finalScore);
    }
    
    /// <summary>
    /// Gets all available level names
    /// </summary>
    public string[] GetLevelNames()
    {
        string[] names = new string[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            names[i] = levels[i].levelName;
        }
        return names;
    }
    
    /// <summary>
    /// Gets level configuration by index
    /// </summary>
    public LevelConfig GetLevel(int index)
    {
        if (index >= 0 && index < levels.Length)
        {
            return levels[index];
        }
        return null;
    }
    
    /// <summary>
    /// Resets the win state (useful for restarting)
    /// </summary>
    public void ResetWinState()
    {
        hasWon = false;
        Debug.Log("[GameLevelManager] Win state reset");
    }
    
    // Public methods for inspector/testing
    [ContextMenu("Set Beginner Level")]
    public void SetBeginnerLevel() => SetLevel(0);
    
    [ContextMenu("Set Intermediate Level")]
    public void SetIntermediateLevel() => SetLevel(1);
    
    [ContextMenu("Set Advanced Level")]
    public void SetAdvancedLevel() => SetLevel(2);
    
    [ContextMenu("Test Victory")]
    public void TestVictory()
    {
        if (Application.isPlaying)
        {
            TriggerVictory(CurrentWinScore);
        }
    }
}