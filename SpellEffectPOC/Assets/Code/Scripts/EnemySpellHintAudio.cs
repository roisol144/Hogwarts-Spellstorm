using UnityEngine;

public class EnemySpellHintAudio : MonoBehaviour
{
    [Header("Hint Audio Clips")]
    [SerializeField] private AudioClip toKillBasilisk;
    [SerializeField] private AudioClip toKillDementor;
    [SerializeField] private AudioClip toKillTroll;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool spatialBlend2D = true; // True for 2D audio, false for 3D positional
    
    // Singleton instance for easy access
    private static EnemySpellHintAudio instance;
    public static EnemySpellHintAudio Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EnemySpellHintAudio>();
                if (instance == null)
                {
                    Debug.LogWarning("[EnemySpellHintAudio] No EnemySpellHintAudio found in scene!");
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // Get or create audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        audioSource.volume = volume;
        audioSource.spatialBlend = spatialBlend2D ? 0f : 1f; // 0 = 2D, 1 = 3D
        audioSource.playOnAwake = false;
        
        Debug.Log("[EnemySpellHintAudio] Initialized enemy spell hint audio system - please assign audio clips manually in Inspector");
    }
    
    /// <summary>
    /// Plays the appropriate hint audio based on enemy type
    /// </summary>
    /// <param name="enemyType">The type of enemy (basilisk, dementor, troll)</param>
    public static void PlayHintForEnemy(string enemyType)
    {
        if (Instance != null)
        {
            Instance.PlayHintAudio(enemyType);
        }
        else
        {
            Debug.LogWarning("[EnemySpellHintAudio] No instance available to play hint audio");
        }
    }
    
    /// <summary>
    /// Plays the hint audio for the specified enemy type
    /// </summary>
    /// <param name="enemyType">The enemy type (basilisk, dementor, troll)</param>
    private void PlayHintAudio(string enemyType)
    {
        AudioClip clipToPlay = null;
        
        switch (enemyType.ToLowerInvariant())
        {
            case "basilisk":
                clipToPlay = toKillBasilisk;
                break;
            case "dementor":
                clipToPlay = toKillDementor;
                break;
            case "troll":
                clipToPlay = toKillTroll;
                break;
            default:
                Debug.LogWarning($"[EnemySpellHintAudio] Unknown enemy type: {enemyType}");
                return;
        }
        
        if (clipToPlay != null && audioSource != null)
        {
            // Stop current audio if playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play the hint audio
            audioSource.clip = clipToPlay;
            audioSource.volume = volume;
            audioSource.Play();
            
            Debug.Log($"[EnemySpellHintAudio] Playing hint audio for {enemyType}: {clipToPlay.name}");
        }
        else
        {
            if (clipToPlay == null)
            {
                Debug.LogWarning($"[EnemySpellHintAudio] No audio clip assigned for enemy type: {enemyType}. Please assign the clip in the Inspector.");
            }
            if (audioSource == null)
            {
                Debug.LogError("[EnemySpellHintAudio] AudioSource is null!");
            }
        }
    }
    
    /// <summary>
    /// Public method to manually set audio clips (for Inspector assignment)
    /// </summary>
    public void SetAudioClips(AudioClip basilisk, AudioClip dementor, AudioClip troll)
    {
        toKillBasilisk = basilisk;
        toKillDementor = dementor;
        toKillTroll = troll;
        Debug.Log("[EnemySpellHintAudio] Audio clips set manually");
    }
    
    /// <summary>
    /// Check if audio is currently playing
    /// </summary>
    public static bool IsPlaying()
    {
        return Instance != null && Instance.audioSource != null && Instance.audioSource.isPlaying;
    }
    
    /// <summary>
    /// Stop currently playing audio
    /// </summary>
    public static void StopAudio()
    {
        if (Instance != null && Instance.audioSource != null && Instance.audioSource.isPlaying)
        {
            Instance.audioSource.Stop();
            Debug.Log("[EnemySpellHintAudio] Stopped hint audio");
        }
    }
    
    /// <summary>
    /// Set the volume for hint audio
    /// </summary>
    public static void SetVolume(float newVolume)
    {
        if (Instance != null)
        {
            Instance.volume = Mathf.Clamp01(newVolume);
            if (Instance.audioSource != null)
            {
                Instance.audioSource.volume = Instance.volume;
            }
            Debug.Log($"[EnemySpellHintAudio] Volume set to: {Instance.volume}");
        }
    }
}