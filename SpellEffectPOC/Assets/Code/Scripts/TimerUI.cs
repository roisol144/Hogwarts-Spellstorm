using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple timer UI that displays collectible challenge countdown, positioned on the left side like ScoreManager on the right
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField] private Canvas timerCanvas; // Will be created if null
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBackground;
    
    [Header("VR UI Positioning")]
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(-0.4f, 0.3f, 0.8f); // Position relative to camera (LEFT side, opposite of score)
    [SerializeField] private float followSpeed = 5f; // How fast to follow (0 = instant, higher = smoother)
    [SerializeField] private bool lookAtCamera = true; // Whether to face the camera
    [SerializeField] private float canvasScale = 0.003f; // Scale of the world space canvas (same as score)
    [SerializeField] private Vector2 canvasSize = new Vector2(200f, 80f); // Canvas size in pixels (smaller than score)
    
    [Header("Visual Effects")]
    [SerializeField] private Color timerTextNormal = new Color(0.8f, 0.9f, 1f, 1f); // Magical silver
    [SerializeField] private Color timerTextWarning = new Color(1f, 0.6f, 0f, 1f); // Magical amber
    [SerializeField] private Color timerTextCritical = new Color(0.8f, 0.1f, 0.1f, 1f); // Deep red
    [SerializeField] private Color backgroundColorNormal = new Color(0f, 0f, 0f, 0f); // Transparent background
    [SerializeField] private float warningTimeThreshold = 30f;
    [SerializeField] private float criticalTimeThreshold = 10f;
    
    // Private variables
    private Camera playerCamera;
    private bool isTimerActive = false;
    private float currentTime = 0f;
    
    // Singleton pattern
    public static TimerUI Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[TimerUI] Multiple TimerUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Setup UI
        SetupTimerUI();
    }
    
    private void Start()
    {
        // Hide timer initially
        HideTimer();
        
        // Subscribe to collectible spawner events
        if (CollectibleSpawner.Instance != null)
        {
            CollectibleSpawner.Instance.OnChallengeStarted += OnChallengeStarted;
            CollectibleSpawner.Instance.OnChallengeCompleted += OnChallengeCompleted;
            CollectibleSpawner.Instance.OnTimerUpdate += OnTimerUpdate;
        }
        
        Debug.Log("[TimerUI] Timer UI initialized.");
    }
    
    private void LateUpdate()
    {
        // Follow the camera smoothly for VR UI
        if (playerCamera != null && timerCanvas != null && isTimerActive)
        {
            FollowCamera();
        }
    }
    
    private void SetupTimerUI()
    {
        if (timerCanvas == null)
        {
            // Create timer canvas for VR (World Space)
            GameObject canvasObject = new GameObject("TimerCanvas_WorldSpace");
            timerCanvas = canvasObject.AddComponent<Canvas>();
            
            // Configure as World Space Canvas (like ScoreManager)
            timerCanvas.renderMode = RenderMode.WorldSpace;
            timerCanvas.sortingOrder = 101; // Slightly higher than score
            
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
            
            // Create timer display
            CreateTimerDisplay();
            
            Debug.Log($"[TimerUI] Created World Space Canvas at position: {canvasObject.transform.position}");
        }
    }
    
    private void CreateTimerDisplay()
    {
        // Timer container
        GameObject timerContainer = new GameObject("TimerContainer");
        timerContainer.transform.SetParent(timerCanvas.transform, false);
        
        RectTransform containerRect = timerContainer.AddComponent<RectTransform>();
        // For WorldSpace canvas, use center anchoring and explicit size
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(180f, 60f); // Timer container (slightly smaller than canvas)
        
        // Background
        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(timerContainer.transform, false);
        timerBackground = bgObject.AddComponent<Image>();
        timerBackground.color = backgroundColorNormal;
        
        RectTransform bgRect = timerBackground.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Timer text
        GameObject textObject = new GameObject("TimerText");
        textObject.transform.SetParent(timerContainer.transform, false);
        timerText = textObject.AddComponent<TextMeshProUGUI>();
        timerText.text = "2:00";
        timerText.fontSize = 36f;
        timerText.color = timerTextNormal;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = timerText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        // Try to load a magical font (same as ScoreManager)
        LoadMagicalFont();
        
        Debug.Log("[TimerUI] Created timer display");
    }
    
    private void LoadMagicalFont()
    {
        // Try to load Harry Potter style font from Resources
        var magicalFont = Resources.Load<TMP_FontAsset>("Fonts/HarryPotter");
        if (magicalFont == null)
        {
            magicalFont = Resources.Load<TMP_FontAsset>("Fonts/MagicalFont");
        }
        if (magicalFont == null)
        {
            magicalFont = Resources.Load<TMP_FontAsset>("Fonts/WizardFont");
        }
        
        if (magicalFont != null && timerText != null)
        {
            timerText.font = magicalFont;
            Debug.Log("[TimerUI] Loaded magical font: " + magicalFont.name);
        }
    }
    
    private void FollowCamera()
    {
        Vector3 targetPosition = GetTargetPosition();
        
        if (followSpeed > 0f)
        {
            // Smooth following
            timerCanvas.transform.position = Vector3.Lerp(
                timerCanvas.transform.position, 
                targetPosition, 
                Time.deltaTime * followSpeed
            );
            
            if (lookAtCamera)
            {
                // Make canvas face the camera
                Vector3 lookDirection = timerCanvas.transform.position - playerCamera.transform.position;
                timerCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        else
        {
            // Instant following
            timerCanvas.transform.position = targetPosition;
            
            if (lookAtCamera)
            {
                Vector3 lookDirection = timerCanvas.transform.position - playerCamera.transform.position;
                timerCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    private Vector3 GetTargetPosition()
    {
        if (playerCamera == null) return transform.position;
        
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        Vector3 cameraUp = playerCamera.transform.up;
        
        return playerCamera.transform.position + 
               cameraForward * offsetFromCamera.z +
               cameraRight * offsetFromCamera.x +
               cameraUp * offsetFromCamera.y;
    }
    
    /// <summary>
    /// Called when a challenge starts
    /// </summary>
    private void OnChallengeStarted(int itemCount, int timeLimit)
    {
        ShowTimer();
        currentTime = timeLimit;
        UpdateTimerDisplay(currentTime);
        Debug.Log($"[TimerUI] Challenge started - showing timer for {timeLimit} seconds");
    }
    
    /// <summary>
    /// Called when timer updates
    /// </summary>
    private void OnTimerUpdate(float timeRemaining)
    {
        currentTime = timeRemaining;
        UpdateTimerDisplay(timeRemaining);
    }
    
    /// <summary>
    /// Called when challenge completes
    /// </summary>
    private void OnChallengeCompleted(bool success)
    {
        HideTimer();
        Debug.Log($"[TimerUI] Challenge completed - hiding timer");
    }
    
    /// <summary>
    /// Updates the timer display with magical color transitions
    /// </summary>
    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText == null) return;
        
        // Format time
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes}:{seconds:00}";
        
        // Color transitions based on time
        if (timeRemaining <= criticalTimeThreshold)
        {
            timerText.color = timerTextCritical;
        }
        else if (timeRemaining <= warningTimeThreshold)
        {
            timerText.color = timerTextWarning;
        }
        else
        {
            timerText.color = timerTextNormal;
        }
    }
    
    /// <summary>
    /// Shows the timer UI
    /// </summary>
    public void ShowTimer()
    {
        if (timerCanvas != null)
        {
            timerCanvas.gameObject.SetActive(true);
            isTimerActive = true;
        }
    }
    
    /// <summary>
    /// Hides the timer UI
    /// </summary>
    public void HideTimer()
    {
        if (timerCanvas != null)
        {
            timerCanvas.gameObject.SetActive(false);
            isTimerActive = false;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CollectibleSpawner.Instance != null)
        {
            CollectibleSpawner.Instance.OnChallengeStarted -= OnChallengeStarted;
            CollectibleSpawner.Instance.OnChallengeCompleted -= OnChallengeCompleted;
            CollectibleSpawner.Instance.OnTimerUpdate -= OnTimerUpdate;
        }
    }
} 