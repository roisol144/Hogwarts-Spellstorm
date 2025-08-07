using UnityEngine;

public class GameAnnouncementAudio : MonoBehaviour
{
    [Header("Announcement Audio Clips")]
    [SerializeField] private AudioClip defeatAnnouncement;
    [SerializeField] private AudioClip collectTheItemAnnouncement;
    [SerializeField] private AudioClip defendTheCastleAnnouncement;
    [SerializeField] private AudioClip hogwartsIsSafeAnnouncement;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool spatialBlend2D = true; // True for 2D audio, false for 3D positional
    
    // Singleton instance for easy access
    private static GameAnnouncementAudio instance;
    public static GameAnnouncementAudio Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameAnnouncementAudio>();
                if (instance == null)
                {
                    Debug.LogWarning("[GameAnnouncementAudio] No GameAnnouncementAudio found in scene!");
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
        
        Debug.Log("[GameAnnouncementAudio] Initialized game announcement audio system - please assign audio clips manually in Inspector");
    }
    
    /// <summary>
    /// Plays the defeat announcement when player dies
    /// </summary>
    public static void PlayDefeatAnnouncement()
    {
        if (Instance != null)
        {
            Instance.PlayAudio(Instance.defeatAnnouncement, "Defeat Announcement");
        }
        else
        {
            Debug.LogWarning("[GameAnnouncementAudio] No instance available to play defeat announcement");
        }
    }
    
    /// <summary>
    /// Plays the collectible announcement when items spawn
    /// </summary>
    public static void PlayCollectTheItemAnnouncement()
    {
        if (Instance != null)
        {
            Instance.PlayAudio(Instance.collectTheItemAnnouncement, "Collect The Item Announcement");
        }
        else
        {
            Debug.LogWarning("[GameAnnouncementAudio] No instance available to play collect item announcement");
        }
    }
    
    /// <summary>
    /// Plays the defend castle announcement when game begins
    /// </summary>
    public static void PlayDefendTheCastleAnnouncement()
    {
        if (Instance != null)
        {
            Instance.PlayAudio(Instance.defendTheCastleAnnouncement, "Defend The Castle Announcement");
        }
        else
        {
            Debug.LogWarning("[GameAnnouncementAudio] No instance available to play defend castle announcement");
        }
    }
    
    /// <summary>
    /// Plays the Hogwarts safe announcement when player wins
    /// </summary>
    public static void PlayHogwartsIsSafeAnnouncement()
    {
        if (Instance != null)
        {
            Instance.PlayAudio(Instance.hogwartsIsSafeAnnouncement, "Hogwarts Is Safe Announcement");
        }
        else
        {
            Debug.LogWarning("[GameAnnouncementAudio] No instance available to play Hogwarts safe announcement");
        }
    }
    
    /// <summary>
    /// Private method to play audio clips
    /// </summary>
    /// <param name="clipToPlay">The audio clip to play</param>
    /// <param name="clipName">Name for debugging</param>
    private void PlayAudio(AudioClip clipToPlay, string clipName)
    {
        if (clipToPlay != null && audioSource != null)
        {
            // Stop current audio if playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play the announcement audio
            audioSource.clip = clipToPlay;
            audioSource.volume = volume;
            audioSource.Play();
            
            Debug.Log($"[GameAnnouncementAudio] Playing {clipName}: {clipToPlay.name}");
        }
        else
        {
            if (clipToPlay == null)
            {
                Debug.LogWarning($"[GameAnnouncementAudio] No audio clip assigned for {clipName}. Please assign the clip in the Inspector.");
            }
            if (audioSource == null)
            {
                Debug.LogError("[GameAnnouncementAudio] AudioSource is null!");
            }
        }
    }
    
    /// <summary>
    /// Public method to manually set audio clips (for Inspector assignment)
    /// </summary>
    public void SetAudioClips(AudioClip defeat, AudioClip collectItem, AudioClip defendCastle = null, AudioClip hogwartsSafe = null)
    {
        defeatAnnouncement = defeat;
        collectTheItemAnnouncement = collectItem;
        if (defendCastle != null) defendTheCastleAnnouncement = defendCastle;
        if (hogwartsSafe != null) hogwartsIsSafeAnnouncement = hogwartsSafe;
        Debug.Log("[GameAnnouncementAudio] Audio clips set manually");
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
            Debug.Log("[GameAnnouncementAudio] Stopped announcement audio");
        }
    }
    
    /// <summary>
    /// Set the volume for announcement audio
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
            Debug.Log($"[GameAnnouncementAudio] Volume set to: {Instance.volume}");
        }
    }
}