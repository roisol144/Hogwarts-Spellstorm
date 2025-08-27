using System.Collections;
using UnityEngine;

/// <summary>
/// Global singleton that handles ML model warmup during scene transitions
/// Ensures the gesture recognition ML model is ready before gameplay begins
/// </summary>
public class MLWarmupManager : MonoBehaviour
{
    public static MLWarmupManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private bool enableLogging = true;
    
    private SentisGestureRecognizer gestureRecognizer;
    private bool hasWarmupCompleted = false;
    
    public bool IsWarmupCompleted => hasWarmupCompleted;
    
    private void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableLogging)
                Debug.Log("[MLWarmupManager] Global ML Warmup Manager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Start ML warmup during scene transition
    /// This should be called when the fade-out begins
    /// </summary>
    public void StartWarmupDuringTransition()
    {
        if (hasWarmupCompleted)
        {
            if (enableLogging)
                Debug.Log("[MLWarmupManager] Warmup already completed, skipping");
            return;
        }
        
        if (enableLogging)
            Debug.Log("[MLWarmupManager] Starting ML warmup during scene transition...");
        
        StartCoroutine(WarmupMLModelCoroutine());
    }
    
    /// <summary>
    /// Force trigger warmup if it hasn't been done yet
    /// This is a fallback for scenes that don't use SceneTransitionManager
    /// </summary>
    public void EnsureWarmupCompleted()
    {
        if (!hasWarmupCompleted)
        {
            if (enableLogging)
                Debug.Log("[MLWarmupManager] Warmup not completed, forcing warmup now...");
            StartCoroutine(WarmupMLModelCoroutine());
        }
    }
    
    private IEnumerator WarmupMLModelCoroutine()
    {
        // Wait one frame to ensure scene loading is stable
        yield return null;
        
        // Find the gesture recognizer in the target scene
        gestureRecognizer = FindObjectOfType<SentisGestureRecognizer>();
        
        if (gestureRecognizer == null)
        {
            if (enableLogging)
                Debug.LogWarning("[MLWarmupManager] SentisGestureRecognizer not found in scene. Warmup skipped.");
            hasWarmupCompleted = true; // Mark as completed to avoid repeated attempts
            yield break;
        }
        
        if (enableLogging)
            Debug.Log("[MLWarmupManager] Found SentisGestureRecognizer, starting warmup...");
        
        // Trigger the warmup on the gesture recognizer
        gestureRecognizer.ForceStartWarmup();
        
        // Wait for warmup to complete
        float timeout = 5f; // Safety timeout
        float elapsed = 0f;
        
        while (!gestureRecognizer.IsWarmedUp && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (gestureRecognizer.IsWarmedUp)
        {
            hasWarmupCompleted = true;
            if (enableLogging)
                Debug.Log("[MLWarmupManager] ✅ ML warmup completed successfully! Gesture recognition ready.");
        }
        else
        {
            Debug.LogError("[MLWarmupManager] ❌ ML warmup timed out! Gesture recognition may be unreliable.");
            hasWarmupCompleted = true; // Mark as completed to avoid infinite retries
        }
    }
    
    /// <summary>
    /// Reset warmup state (useful for testing or scene reloads)
    /// </summary>
    public void ResetWarmupState()
    {
        hasWarmupCompleted = false;
        gestureRecognizer = null;
        
        if (enableLogging)
            Debug.Log("[MLWarmupManager] Warmup state reset");
    }
}
