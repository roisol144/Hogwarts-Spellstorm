using UnityEngine;

/// <summary>
/// Automatic setup script for background music system
/// Can be added to any GameObject to automatically create and configure BackgroundMusicManager
/// </summary>
public class BackgroundMusicSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createIfNotExists = true;
    [SerializeField] private bool destroyAfterSetup = true;
    
    [Header("Default Music Settings")]
    [SerializeField] private AudioClip defaultBackgroundMusic;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.5f;
    [SerializeField] private bool enableFadeIn = true;
    [SerializeField] private bool enableFadeOut = true;
    
    [Header("Scene-Specific Music")]
    [SerializeField] private AudioClip dungeonMusic;
    [SerializeField] private AudioClip chamberMusic;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupBackgroundMusic();
        }
    }
    
    /// <summary>
    /// Sets up the background music system
    /// </summary>
    [ContextMenu("Setup Background Music")]
    public void SetupBackgroundMusic()
    {
        Debug.Log("[BackgroundMusicSetup] Setting up background music system...");
        
        // Check if BackgroundMusicManager already exists
        BackgroundMusicManager existingManager = BackgroundMusicManager.Instance;
        
        if (existingManager != null)
        {
            Debug.Log("[BackgroundMusicSetup] BackgroundMusicManager already exists, configuring...");
            ConfigureExistingManager(existingManager);
        }
        else if (createIfNotExists)
        {
            Debug.Log("[BackgroundMusicSetup] Creating new BackgroundMusicManager...");
            CreateBackgroundMusicManager();
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicSetup] No BackgroundMusicManager found and createIfNotExists is false!");
        }
        
        // Destroy this setup script if requested
        if (destroyAfterSetup)
        {
            Debug.Log("[BackgroundMusicSetup] Setup complete, destroying setup script.");
            Destroy(this);
        }
    }
    
    /// <summary>
    /// Creates a new BackgroundMusicManager GameObject
    /// </summary>
    private void CreateBackgroundMusicManager()
    {
        // Create GameObject
        GameObject musicManagerGO = new GameObject("BackgroundMusicManager");
        
        // Add BackgroundMusicManager component
        BackgroundMusicManager musicManager = musicManagerGO.AddComponent<BackgroundMusicManager>();
        
        // Configure the manager
        ConfigureMusicManager(musicManager);
        
        Debug.Log("[BackgroundMusicSetup] Created BackgroundMusicManager successfully!");
    }
    
    /// <summary>
    /// Configures an existing BackgroundMusicManager
    /// </summary>
    private void ConfigureExistingManager(BackgroundMusicManager manager)
    {
        ConfigureMusicManager(manager);
        Debug.Log("[BackgroundMusicSetup] Configured existing BackgroundMusicManager!");
    }
    
    /// <summary>
    /// Configures the music manager with appropriate settings
    /// </summary>
    private void ConfigureMusicManager(BackgroundMusicManager manager)
    {
        // Get current scene name
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Determine which music to use based on scene
        AudioClip musicToUse = GetMusicForScene(currentScene);
        
        if (musicToUse != null)
        {
            // Use reflection to set the private field (since it's not public)
            var musicField = typeof(BackgroundMusicManager).GetField("backgroundMusic", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (musicField != null)
            {
                musicField.SetValue(manager, musicToUse);
                Debug.Log($"[BackgroundMusicSetup] Set music for scene '{currentScene}': {musicToUse.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[BackgroundMusicSetup] No music assigned for scene '{currentScene}'");
        }
        
        // Set volume
        manager.SetVolume(defaultVolume);
        
        Debug.Log($"[BackgroundMusicSetup] Configured music manager with volume: {defaultVolume:F2}");
    }
    
    /// <summary>
    /// Gets the appropriate music clip for the current scene
    /// </summary>
    private AudioClip GetMusicForScene(string sceneName)
    {
        string sceneLower = sceneName.ToLower();
        
        if (sceneLower.Contains("dungeon"))
        {
            return dungeonMusic != null ? dungeonMusic : defaultBackgroundMusic;
        }
        else if (sceneLower.Contains("chamber"))
        {
            return chamberMusic != null ? chamberMusic : defaultBackgroundMusic;
        }
        else
        {
            return defaultBackgroundMusic;
        }
    }
    
    /// <summary>
    /// Context menu method for manual setup
    /// </summary>
    [ContextMenu("Manual Setup")]
    private void ManualSetup()
    {
        SetupBackgroundMusic();
    }
    
    /// <summary>
    /// Context menu method for testing music
    /// </summary>
    [ContextMenu("Test Music")]
    private void TestMusic()
    {
        BackgroundMusicManager manager = BackgroundMusicManager.Instance;
        if (manager != null)
        {
            if (manager.IsPlaying)
            {
                manager.StopMusic();
                Debug.Log("[BackgroundMusicSetup] Stopped music for testing");
            }
            else
            {
                manager.PlayMusic();
                Debug.Log("[BackgroundMusicSetup] Started music for testing");
            }
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicSetup] No BackgroundMusicManager found for testing!");
        }
    }
}
