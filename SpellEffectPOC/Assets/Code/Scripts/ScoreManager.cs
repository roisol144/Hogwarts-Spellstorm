using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int pointsPerKill = 10;
    
    [Header("Score UI")]
    [SerializeField] private Canvas scoreCanvas; // Will be created if null
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreBackground;
    [SerializeField] private TMP_FontAsset customFont; // Custom font to use (if null, will try to auto-load magical font)
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Vector2 textPadding = new Vector2(10f, 10f); // Padding from container edges
    
    [Header("VR UI Positioning")]
    [SerializeField] private bool enableCameraFollowingUI = true; // ENABLED: Display like TimerUI but on right side
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(0.4f, 0.3f, 0.8f); // Position relative to camera (right, above spell debug, forward)
    [SerializeField] private float followSpeed = 5f; // How fast to follow (0 = instant, higher = smoother)
    [SerializeField] private bool lookAtCamera = true; // Whether to face the camera
    [SerializeField] private float canvasScale = 0.003f; // Scale of the world space canvas (same as spell debug)
    [SerializeField] private Vector2 canvasSize = new Vector2(200f, 80f); // Canvas size in pixels (same as timer)
    
    [Header("Visual Effects")]
    [SerializeField] private Color scoreTextColor = new Color(1f, 0.84f, 0f, 1f); // Golden color
    [SerializeField] private Color backgroundColorNormal = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color backgroundColorHighlight = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    [SerializeField] private float highlightDuration = 0.5f;
    [SerializeField] private AnimationCurve highlightCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    [Header("Audio")]
    [SerializeField] private AudioClip scoreGainSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private Camera playerCamera;
    private Coroutine highlightCoroutine;
    
    // Events
    public System.Action<int> OnScoreChanged; // int = new score
    public System.Action<int> OnPointsGained; // int = points gained
    
    // Singleton pattern for easy access
    public static ScoreManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[ScoreManager] Multiple ScoreManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup UI
        SetupScoreUI();
    }
    
    private void Start()
    {
        UpdateScoreUI();
        OnScoreChanged?.Invoke(currentScore);
    }
    
    private void LateUpdate()
    {
        // Follow the camera smoothly for VR UI (only if camera-following UI is enabled)
        if (enableCameraFollowingUI && playerCamera != null && scoreCanvas != null)
        {
            FollowCamera();
        }
    }
    
    private void SetupScoreUI()
    {
        // Only create camera-following UI if enabled (disabled by default in favor of StaticScoreDisplay)
        if (scoreCanvas == null && enableCameraFollowingUI)
        {
            // Create score canvas for VR (World Space)
            GameObject canvasObject = new GameObject("ScoreCanvas_WorldSpace");
            scoreCanvas = canvasObject.AddComponent<Canvas>();
            
            // Configure as World Space Canvas (like MagicalDebugUI)
            scoreCanvas.renderMode = RenderMode.WorldSpace;
            scoreCanvas.sortingOrder = 100;
            
            // Set initial position near camera
            if (playerCamera != null)
            {
                canvasObject.transform.position = GetTargetPosition();
                if (lookAtCamera)
                {
                    canvasObject.transform.LookAt(playerCamera.transform);
                }
            }
            
            // Set canvas scale
            canvasObject.transform.localScale = Vector3.one * canvasScale;
            
            // Set canvas size
            var rectTransform = canvasObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = canvasSize;
            
            // Add CanvasScaler for consistent scaling
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            
            // Add GraphicRaycaster
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Create score display
            CreateScoreDisplay();
            
            Debug.Log($"[ScoreManager] Created World Space Canvas at position: {canvasObject.transform.position}");
        }
    }
    
    private void CreateScoreDisplay()
    {
        // Score container
        GameObject scoreContainer = new GameObject("ScoreContainer");
        scoreContainer.transform.SetParent(scoreCanvas.transform, false);
        
        RectTransform containerRect = scoreContainer.AddComponent<RectTransform>();
        // For WorldSpace canvas, use center anchoring and explicit size
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(180f, 60f); // Score container (same size as timer)
        
        // Background
        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(scoreContainer.transform, false);
        scoreBackground = bgObject.AddComponent<Image>();
        scoreBackground.color = backgroundColorNormal;
        
        RectTransform bgRect = scoreBackground.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Score text
        GameObject textObject = new GameObject("ScoreText");
        textObject.transform.SetParent(scoreContainer.transform, false);
        scoreText = textObject.AddComponent<TextMeshProUGUI>();
        scoreText.text = "Score: 0";
        scoreText.fontSize = fontSize;
        scoreText.color = scoreTextColor;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = scoreText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textPadding;
        textRect.offsetMax = -textPadding;
        
        // Try to load a magical font (same as MagicalDebugUI)
        LoadMagicalFont();
        
        Debug.Log("[ScoreManager] Created score display");
    }
    
    private void LoadMagicalFont()
    {
        TMP_FontAsset fontToUse = null;
        
        // First priority: Use custom font if assigned
        if (customFont != null)
        {
            fontToUse = customFont;
        }
        else
        {
            // Fallback: Try to load Harry Potter style font from Resources
            fontToUse = Resources.Load<TMP_FontAsset>("Fonts/HarryPotter");
            if (fontToUse == null)
            {
                fontToUse = Resources.Load<TMP_FontAsset>("Fonts/MagicalFont");
            }
            if (fontToUse == null)
            {
                fontToUse = Resources.Load<TMP_FontAsset>("Fonts/WizardFont");
            }
        }
        
        if (fontToUse != null && scoreText != null)
        {
            scoreText.font = fontToUse;
            Debug.Log("[ScoreManager] Loaded font: " + fontToUse.name);
        }
        else if (customFont == null)
        {
            Debug.LogWarning("[ScoreManager] No custom font assigned and no magical font found in Resources.");
        }
    }

    /// <summary>
    /// Applies current font and size settings to the score text. Useful for runtime changes.
    /// </summary>
    public void RefreshTextSettings()
    {
        if (scoreText != null)
        {
            LoadMagicalFont();
            scoreText.fontSize = fontSize;
        }
    }
    
    private Vector3 GetTargetPosition()
    {
        if (playerCamera == null) return Vector3.zero;
        
        // Calculate position relative to camera (top right corner)
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        Vector3 cameraUp = playerCamera.transform.up;
        
        return playerCamera.transform.position +
               cameraForward * offsetFromCamera.z +
               cameraRight * offsetFromCamera.x +
               cameraUp * offsetFromCamera.y;
    }
    
    private void FollowCamera()
    {
        Vector3 targetPosition = GetTargetPosition();
        
        // Smooth following
        if (followSpeed > 0f)
        {
            scoreCanvas.transform.position = Vector3.Lerp(
                scoreCanvas.transform.position, 
                targetPosition, 
                Time.deltaTime * followSpeed
            );
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = scoreCanvas.transform.position - playerCamera.transform.position;
                scoreCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        else
        {
            // Instant following
            scoreCanvas.transform.position = targetPosition;
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = scoreCanvas.transform.position - playerCamera.transform.position;
                scoreCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    public void AddScore(int points)
    {
        int previousScore = currentScore;
        currentScore += points;
        
        Debug.Log($"[ScoreManager] Score increased: {previousScore} → {currentScore} (+{points})");
        
        // Play score gain sound
        if (scoreGainSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreGainSound);
        }
        
        // Trigger visual highlight effect
        TriggerHighlightEffect();
        
        // Update UI
        UpdateScoreUI();
        
        // Trigger events
        OnScoreChanged?.Invoke(currentScore);
        OnPointsGained?.Invoke(points);
    }
    
    public void AddKillScore()
    {
        AddScore(pointsPerKill);
    }
    
    private void TriggerHighlightEffect()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }
        highlightCoroutine = StartCoroutine(HighlightEffectCoroutine());
    }
    
    private IEnumerator HighlightEffectCoroutine()
    {
        if (scoreBackground == null) yield break;
        
        Color originalColor = backgroundColorNormal;
        Color highlightColor = backgroundColorHighlight;
        
        float elapsedTime = 0f;
        while (elapsedTime < highlightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / highlightDuration;
            float intensity = highlightCurve.Evaluate(t);
            
            scoreBackground.color = Color.Lerp(originalColor, highlightColor, intensity);
            
            yield return null;
        }
        
        // Restore original color
        scoreBackground.color = originalColor;
        highlightCoroutine = null;
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
            Debug.Log($"[ScoreManager] Updated score display to: {currentScore}");
        }
        else
        {
            Debug.LogError("[ScoreManager] scoreText is NULL! Score won't update visually.");
        }
    }
    
    // Public getters and setters
    public int GetCurrentScore() => currentScore;
    public int GetPointsPerKill() => pointsPerKill;
    public void SetPointsPerKill(int points) => pointsPerKill = Mathf.Max(0, points);
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log("[ScoreManager] Score reset to 0");
    }
    
    // Static method for easy access from other scripts
    public static void NotifyKill()
    {
        Debug.Log("[ScoreManager] NotifyKill() called");
        if (Instance != null)
        {
            Debug.Log($"[ScoreManager] ScoreManager instance found, awarding {Instance.pointsPerKill} points");
            Instance.AddKillScore();
        }
        else
        {
            Debug.LogWarning("[ScoreManager] No ScoreManager instance found! Cannot add kill score.");
        }
    }
    
    public static void NotifyScore(int points)
    {
        if (Instance != null)
        {
            Instance.AddScore(points);
        }
        else
        {
            Debug.LogWarning("[ScoreManager] No ScoreManager instance found! Cannot add score.");
        }
    }
    
    /// <summary>
    /// Reduces score as a penalty for wrong actions (like using wrong spells)
    /// </summary>
    public static void NotifyPenalty(int penaltyPoints, string reason = "")
    {
        if (Instance != null)
        {
            int actualPenalty = -Mathf.Abs(penaltyPoints); // Ensure it's negative
            Debug.Log($"[ScoreManager] Applying penalty: {actualPenalty} points. Reason: {reason}");
            Instance.AddScore(actualPenalty);
        }
        else
        {
            Debug.LogWarning("[ScoreManager] No ScoreManager instance found! Cannot apply penalty.");
        }
    }
} 