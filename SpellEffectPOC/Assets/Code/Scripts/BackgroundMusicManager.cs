using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Background music manager for game scenes (Dungeon, Chamber of Secrets)
/// Provides Inspector-controlled volume, fade effects, and scene-specific music
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource musicAudioSource;
    
    [Header("Volume Control")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopMusic = true;
    
    [Header("Fade Effects")]
    [SerializeField] private bool enableFadeIn = true;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private bool enableFadeOut = true;
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("Scene-Specific Settings")]
    [SerializeField] private bool autoDetectScene = true;
    [SerializeField] private string dungeonSceneName = "DungeonsScene";
    [SerializeField] private string chamberSceneName = "ChamberOfSecretsScene";
    
    [Header("UI Controls (Optional)")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button muteButton;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Sprite mutedSprite;
    [SerializeField] private Sprite unmutedSprite;
    
    [Header("Audio Settings")]
    [SerializeField] private bool spatialBlend2D = true; // True for 2D background music
    [SerializeField] private int priority = 128; // Lower number = higher priority
    
    // Private variables
    private bool isMuted = false;
    private float originalVolume;
    private Coroutine fadeCoroutine;
    private bool isInitialized = false;
    
    // Singleton pattern for easy access
    public static BackgroundMusicManager Instance { get; private set; }
    
    // Events
    public System.Action<float> OnVolumeChanged; // float = new volume
    public System.Action<bool> OnMuteChanged; // bool = is muted
    
    // Properties
    public bool IsPlaying => musicAudioSource != null && musicAudioSource.isPlaying;
    public bool IsMuted => isMuted;
    public float CurrentVolume => musicVolume;
    public AudioClip CurrentMusic => backgroundMusic;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[BackgroundMusicManager] Multiple instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        // Setup UI controls if available
        SetupUIControls();
        
        // Start playing if enabled
        if (playOnStart)
        {
            PlayMusic();
        }
        
        Debug.Log($"[BackgroundMusicManager] Initialized. Scene: {GetCurrentSceneName()}, Volume: {musicVolume:F2}");
    }
    
    private void Initialize()
    {
        // Setup audio source
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        ConfigureAudioSource();
        
        // Store original volume
        originalVolume = musicVolume;
        
        isInitialized = true;
    }
    
    private void ConfigureAudioSource()
    {
        if (musicAudioSource == null) return;
        
        musicAudioSource.clip = backgroundMusic;
        musicAudioSource.volume = isMuted ? 0f : musicVolume;
        musicAudioSource.loop = loopMusic;
        musicAudioSource.playOnAwake = false; // We'll control this manually
        musicAudioSource.spatialBlend = spatialBlend2D ? 0f : 1f; // 0 = 2D, 1 = 3D
        musicAudioSource.priority = priority;
        
        // Set audio mixer group if available
        // musicAudioSource.outputAudioMixerGroup = yourMixerGroup;
    }
    
    private void SetupUIControls()
    {
        // Setup volume slider
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        
        // Setup mute button
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
            UpdateMuteButtonVisual();
        }
        
        // Update volume text
        UpdateVolumeText();
    }
    
    /// <summary>
    /// Plays the background music with optional fade-in
    /// </summary>
    public void PlayMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("[BackgroundMusicManager] No background music assigned!");
            return;
        }
        
        if (musicAudioSource == null)
        {
            Debug.LogError("[BackgroundMusicManager] AudioSource is null!");
            return;
        }
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Configure audio source
        ConfigureAudioSource();
        
        // Start playing
        musicAudioSource.Play();
        
        // Apply fade-in if enabled
        if (enableFadeIn && !isMuted)
        {
            fadeCoroutine = StartCoroutine(FadeInCoroutine());
        }
        
        Debug.Log($"[BackgroundMusicManager] Started playing background music: {backgroundMusic.name}");
    }
    
    /// <summary>
    /// Stops the background music with optional fade-out
    /// </summary>
    public void StopMusic()
    {
        if (musicAudioSource == null) return;
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (enableFadeOut)
        {
            fadeCoroutine = StartCoroutine(FadeOutCoroutine());
        }
        else
        {
            musicAudioSource.Stop();
        }
        
        Debug.Log("[BackgroundMusicManager] Stopped background music");
    }
    
    /// <summary>
    /// Pauses the background music
    /// </summary>
    public void PauseMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Pause();
            Debug.Log("[BackgroundMusicManager] Paused background music");
        }
    }
    
    /// <summary>
    /// Resumes the background music
    /// </summary>
    public void ResumeMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.UnPause();
            Debug.Log("[BackgroundMusicManager] Resumed background music");
        }
    }
    
    /// <summary>
    /// Sets the music volume (0-1)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        musicVolume = Mathf.Clamp01(newVolume);
        
        if (musicAudioSource != null && !isMuted)
        {
            musicAudioSource.volume = musicVolume;
        }
        
        // Update UI
        if (volumeSlider != null && Mathf.Abs(volumeSlider.value - musicVolume) > 0.01f)
        {
            volumeSlider.value = musicVolume;
        }
        
        UpdateVolumeText();
        
        // Notify listeners
        OnVolumeChanged?.Invoke(musicVolume);
        
        Debug.Log($"[BackgroundMusicManager] Volume set to {musicVolume:F2}");
    }
    
    /// <summary>
    /// Toggles mute state
    /// </summary>
    public void ToggleMute()
    {
        SetMuted(!isMuted);
    }
    
    /// <summary>
    /// Sets mute state
    /// </summary>
    public void SetMuted(bool muted)
    {
        isMuted = muted;
        
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = muted ? 0f : musicVolume;
        }
        
        UpdateMuteButtonVisual();
        
        // Notify listeners
        OnMuteChanged?.Invoke(isMuted);
        
        Debug.Log($"[BackgroundMusicManager] Mute {(muted ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Changes the background music clip
    /// </summary>
    public void ChangeMusic(AudioClip newMusic, bool playImmediately = true)
    {
        backgroundMusic = newMusic;
        
        if (musicAudioSource != null)
        {
            musicAudioSource.clip = newMusic;
            
            if (playImmediately && IsPlaying)
            {
                musicAudioSource.Play();
            }
        }
        
        Debug.Log($"[BackgroundMusicManager] Changed music to: {(newMusic != null ? newMusic.name : "null")}");
    }
    
    /// <summary>
    /// Fades the music to a specific volume over time
    /// </summary>
    public void FadeToVolume(float targetVolume, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeToVolumeCoroutine(targetVolume, duration));
    }
    
    /// <summary>
    /// Gets the current scene name
    /// </summary>
    private string GetCurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Updates the volume text display
    /// </summary>
    private void UpdateVolumeText()
    {
        if (volumeText != null)
        {
            volumeText.text = $"Volume: {(int)(musicVolume * 100)}%";
        }
    }
    
    /// <summary>
    /// Updates the mute button visual
    /// </summary>
    private void UpdateMuteButtonVisual()
    {
        if (muteButtonImage != null)
        {
            muteButtonImage.sprite = isMuted ? mutedSprite : unmutedSprite;
        }
    }
    
    /// <summary>
    /// Fade-in coroutine
    /// </summary>
    private System.Collections.IEnumerator FadeInCoroutine()
    {
        float startVolume = 0f;
        float targetVolume = isMuted ? 0f : musicVolume;
        float elapsedTime = 0f;
        
        musicAudioSource.volume = startVolume;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        musicAudioSource.volume = targetVolume;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Fade-out coroutine
    /// </summary>
    private System.Collections.IEnumerator FadeOutCoroutine()
    {
        float startVolume = musicAudioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        musicAudioSource.Stop();
        musicAudioSource.volume = isMuted ? 0f : musicVolume;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Fade to specific volume coroutine
    /// </summary>
    private System.Collections.IEnumerator FadeToVolumeCoroutine(float targetVolume, float duration)
    {
        float startVolume = musicAudioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            musicAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        musicAudioSource.volume = targetVolume;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Context menu method for testing
    /// </summary>
    [ContextMenu("Test Play Music")]
    private void TestPlayMusic()
    {
        PlayMusic();
    }
    
    /// <summary>
    /// Context menu method for testing
    /// </summary>
    [ContextMenu("Test Stop Music")]
    private void TestStopMusic()
    {
        StopMusic();
    }
    
    /// <summary>
    /// Context menu method for testing
    /// </summary>
    [ContextMenu("Test Fade To 0")]
    private void TestFadeToZero()
    {
        FadeToVolume(0f, 2f);
    }
    
    /// <summary>
    /// Context menu method for testing
    /// </summary>
    [ContextMenu("Test Fade To Full")]
    private void TestFadeToFull()
    {
        FadeToVolume(musicVolume, 2f);
    }
    
    private void OnDestroy()
    {
        // Stop any running coroutines
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
    
    private void OnValidate()
    {
        // Update volume in editor if audio source exists
        if (musicAudioSource != null && !isMuted)
        {
            musicAudioSource.volume = musicVolume;
        }
    }
}
