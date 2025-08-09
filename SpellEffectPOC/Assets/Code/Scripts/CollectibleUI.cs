using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Magical UI system for displaying collectible challenge information with Harry Potter theming
/// </summary>
public class CollectibleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI challengeMessageText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject challengePanel;
    [SerializeField] private Image magicalBorder;
    [SerializeField] private Image backgroundGlow;
    
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont; // Custom font to use (if null, will try to auto-load magical font)
    [SerializeField] private float timerFontSize = 42f;
    [SerializeField] private float progressFontSize = 28f;
    [SerializeField] private float messageFontSize = 22f;
    [SerializeField] private Vector2 textPadding = new Vector2(15f, 5f); // Padding from container edges
    
    [Header("VR UI Positioning")]
    [SerializeField] private bool enableCameraFollowingUI = true; // Enable camera following behavior
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(-0.6f, 0.2f, 1.2f); // More subtle positioning
    [SerializeField] private float followSpeed = 3f; // Slower, more graceful movement
    [SerializeField] private bool lookAtCamera = true;
    [SerializeField] private float canvasScale = 0.002f; // Smaller, more subtle
    [SerializeField] private Vector2 canvasSize = new Vector2(450f, 220f);
    
    [Header("Magical Colors")]
    [SerializeField] private Color magicalGold = new Color(1f, 0.84f, 0f, 1f);          // Hogwarts gold
    [SerializeField] private Color mysticalBlue = new Color(0.1f, 0.2f, 0.6f, 0.85f);   // Deep magical blue
    [SerializeField] private Color enchantedSilver = new Color(0.8f, 0.9f, 1f, 1f);     // Magical silver
    [SerializeField] private Color warningAmber = new Color(1f, 0.6f, 0f, 1f);          // Magical amber
    [SerializeField] private Color criticalCrimson = new Color(0.8f, 0.1f, 0.1f, 1f);   // Deep red
    [SerializeField] private Color successEmerald = new Color(0.2f, 0.8f, 0.3f, 1f);    // Magical green
    
    [Header("Magical Effects")]
    [SerializeField] private float warningTimeThreshold = 30f; // 30 seconds
    [SerializeField] private float criticalTimeThreshold = 10f; // 10 seconds
    [SerializeField] private AnimationCurve shimmerCurve = AnimationCurve.EaseInOut(0, 0.7f, 1, 1f);
    [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float shimmerSpeed = 2f;
    [SerializeField] private float floatAmount = 0.1f;
    
    // Private variables
    private Camera playerCamera;
    private CollectibleChallenge currentChallenge;
    private Coroutine uiUpdateCoroutine;
    private Coroutine magicalEffectsCoroutine;
    private Coroutine shimmerCoroutine;
    private bool isVisible = false;
    
    // Singleton pattern
    public static CollectibleUI Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[CollectibleUI] Multiple instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
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
        // Check if TimerUI exists - if so, disable this complex UI in favor of the simple one
        TimerUI timerUI = FindObjectOfType<TimerUI>();
        if (timerUI != null)
        {
            Debug.Log("[CollectibleUI] TimerUI detected - disabling complex CollectibleUI in favor of simple timer.");
            gameObject.SetActive(false);
            return;
        }
        
        // Setup UI if not already configured
        if (uiCanvas == null)
        {
            SetupMagicalUI();
        }
        
        // Hide UI initially
        HideUI();
        
        // Subscribe to collectible spawner events
        if (CollectibleSpawner.Instance != null)
        {
            CollectibleSpawner.Instance.OnChallengeStarted += OnChallengeStarted;
            CollectibleSpawner.Instance.OnItemCollected += OnItemCollected;
            CollectibleSpawner.Instance.OnChallengeCompleted += OnChallengeCompleted;
            CollectibleSpawner.Instance.OnTimerUpdate += OnTimerUpdate;
        }
        
        Debug.Log("[CollectibleUI] Magical Collectible UI initialized.");
    }
    
    void LateUpdate()
    {
        // Follow the camera smoothly for VR UI with gentle floating motion (only if enabled)
        if (enableCameraFollowingUI && playerCamera != null && uiCanvas != null && isVisible)
        {
            FollowCameraWithMagicalMotion();
        }
    }
    
    /// <summary>
    /// Sets up the magical themed UI canvas and components
    /// </summary>
    private void SetupMagicalUI()
    {
        // Create canvas with magical properties
        GameObject canvasObject = new GameObject("MagicalCollectibleUICanvas");
        canvasObject.transform.SetParent(transform, false);
        uiCanvas = canvasObject.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;
        uiCanvas.worldCamera = playerCamera;
        uiCanvas.sortingOrder = 150; // Subtle layering
        
        // Setup canvas properties
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = canvasSize;
        canvasRect.localScale = Vector3.one * canvasScale;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        // Create magical challenge panel
        CreateMagicalChallengePanel();
        
        Debug.Log("[CollectibleUI] Magical UI setup completed.");
    }
    
    /// <summary>
    /// Creates the magical themed challenge panel
    /// </summary>
    private void CreateMagicalChallengePanel()
    {
        // Main challenge panel
        GameObject panelObject = new GameObject("MagicalChallengePanel");
        panelObject.transform.SetParent(uiCanvas.transform, false);
        challengePanel = panelObject;
        
        // Background glow effect
        backgroundGlow = panelObject.AddComponent<Image>();
        backgroundGlow.color = new Color(mysticalBlue.r, mysticalBlue.g, mysticalBlue.b, 0.6f);
        backgroundGlow.sprite = CreateRoundedRectSprite();
        
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(420f, 200f);
        
        // Magical border
        GameObject borderObject = new GameObject("MagicalBorder");
        borderObject.transform.SetParent(panelObject.transform, false);
        magicalBorder = borderObject.AddComponent<Image>();
        magicalBorder.color = magicalGold;
        magicalBorder.sprite = CreateBorderSprite();
        
        RectTransform borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        // Magical timer text with glow
        CreateMagicalTimerText(panelObject);
        
        // Elegant progress text
        CreateMagicalProgressText(panelObject);
        
        // Mystical progress bar
        CreateMagicalProgressBar(panelObject);
        
        // Enchanted message text
        CreateMagicalMessageText(panelObject);
        
        // Load custom fonts for all text components
        LoadMagicalFonts();
    }
    
    /// <summary>
    /// Creates magical timer text with glowing effects
    /// </summary>
    private void CreateMagicalTimerText(GameObject parent)
    {
        GameObject timerObject = new GameObject("MagicalTimer");
        timerObject.transform.SetParent(parent.transform, false);
        timerText = timerObject.AddComponent<TextMeshProUGUI>();
        
        // Configure magical timer appearance
        timerText.text = "2:00";
        timerText.fontSize = timerFontSize;
        timerText.color = enchantedSilver;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontStyle = FontStyles.Bold;
        
        // Add magical glow outline
        timerText.fontMaterial = CreateGlowMaterial(enchantedSilver);
        
        RectTransform timerRect = timerText.rectTransform;
        timerRect.anchorMin = new Vector2(0f, 0.65f);
        timerRect.anchorMax = new Vector2(1f, 0.95f);
        timerRect.offsetMin = textPadding;
        timerRect.offsetMax = -textPadding;
    }
    
    /// <summary>
    /// Creates elegant progress text
    /// </summary>
    private void CreateMagicalProgressText(GameObject parent)
    {
        GameObject progressObject = new GameObject("MagicalProgress");
        progressObject.transform.SetParent(parent.transform, false);
        progressText = progressObject.AddComponent<TextMeshProUGUI>();
        
        progressText.text = "0 of 4 Treasures Found";
        progressText.fontSize = progressFontSize;
        progressText.color = magicalGold;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.fontStyle = FontStyles.Normal;
        
        // Add subtle glow
        progressText.fontMaterial = CreateGlowMaterial(magicalGold, 0.3f);
        
        RectTransform progressRect = progressText.rectTransform;
        progressRect.anchorMin = new Vector2(0f, 0.4f);
        progressRect.anchorMax = new Vector2(1f, 0.65f);
        progressRect.offsetMin = textPadding;
        progressRect.offsetMax = -textPadding;
    }
    
    /// <summary>
    /// Creates mystical progress bar with magical effects
    /// </summary>
    private void CreateMagicalProgressBar(GameObject parent)
    {
        // Progress bar background
        GameObject progressBarBg = new GameObject("MysticalProgressBackground");
        progressBarBg.transform.SetParent(parent.transform, false);
        Image progressBarBgImage = progressBarBg.AddComponent<Image>();
        progressBarBgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        progressBarBgImage.sprite = CreateRoundedRectSprite();
        
        RectTransform progressBarBgRect = progressBarBg.GetComponent<RectTransform>();
        progressBarBgRect.anchorMin = new Vector2(0.1f, 0.25f);
        progressBarBgRect.anchorMax = new Vector2(0.9f, 0.38f);
        progressBarBgRect.offsetMin = Vector2.zero;
        progressBarBgRect.offsetMax = Vector2.zero;
        
        // Magical progress bar fill
        GameObject progressBarObject = new GameObject("MysticalProgressBar");
        progressBarObject.transform.SetParent(progressBarBg.transform, false);
        progressBar = progressBarObject.AddComponent<Image>();
        progressBar.color = successEmerald;
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.sprite = CreateRoundedRectSprite();
        
        RectTransform progressBarRect = progressBar.rectTransform;
        progressBarRect.anchorMin = Vector2.zero;
        progressBarRect.anchorMax = Vector2.one;
        progressBarRect.offsetMin = new Vector2(2, 2);
        progressBarRect.offsetMax = new Vector2(-2, -2);
    }
    
    /// <summary>
    /// Creates enchanted message text
    /// </summary>
    private void CreateMagicalMessageText(GameObject parent)
    {
        GameObject messageObject = new GameObject("EnchantedMessage");
        messageObject.transform.SetParent(parent.transform, false);
        challengeMessageText = messageObject.AddComponent<TextMeshProUGUI>();
        
        challengeMessageText.text = "Seek the magical treasures...";
        challengeMessageText.fontSize = messageFontSize;
        challengeMessageText.color = new Color(magicalGold.r, magicalGold.g, magicalGold.b, 0.9f);
        challengeMessageText.alignment = TextAlignmentOptions.Center;
        challengeMessageText.fontStyle = FontStyles.Italic;
        
        // Add gentle glow
        challengeMessageText.fontMaterial = CreateGlowMaterial(magicalGold, 0.2f);
        
        RectTransform messageRect = challengeMessageText.rectTransform;
        messageRect.anchorMin = new Vector2(0f, 0.05f);
        messageRect.anchorMax = new Vector2(1f, 0.25f);
        messageRect.offsetMin = textPadding;
        messageRect.offsetMax = -textPadding;
    }
    
    /// <summary>
    /// Creates a material with magical glow effect
    /// </summary>
    private Material CreateGlowMaterial(Color glowColor, float glowPower = 0.5f)
    {
        // For now, return null - this would require custom shader setup
        // In a full implementation, you'd create a material with glow shader
        return null;
    }
    
    /// <summary>
    /// Loads custom fonts for all text components
    /// </summary>
    private void LoadMagicalFonts()
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
        
        if (fontToUse != null)
        {
            if (timerText != null) timerText.font = fontToUse;
            if (progressText != null) progressText.font = fontToUse;
            if (challengeMessageText != null) challengeMessageText.font = fontToUse;
            Debug.Log("[CollectibleUI] Loaded font: " + fontToUse.name);
        }
        else if (customFont == null)
        {
            Debug.LogWarning("[CollectibleUI] No custom font assigned and no magical font found in Resources.");
        }
    }
    
    /// <summary>
    /// Applies current font and size settings to all text components. Useful for runtime changes.
    /// </summary>
    public void RefreshTextSettings()
    {
        if (timerText != null)
        {
            timerText.fontSize = timerFontSize;
            var timerRect = timerText.rectTransform;
            timerRect.offsetMin = textPadding;
            timerRect.offsetMax = -textPadding;
        }
        
        if (progressText != null)
        {
            progressText.fontSize = progressFontSize;
            var progressRect = progressText.rectTransform;
            progressRect.offsetMin = textPadding;
            progressRect.offsetMax = -textPadding;
        }
        
        if (challengeMessageText != null)
        {
            challengeMessageText.fontSize = messageFontSize;
            var messageRect = challengeMessageText.rectTransform;
            messageRect.offsetMin = textPadding;
            messageRect.offsetMax = -textPadding;
        }
        
        LoadMagicalFonts();
    }
    
    private void OnValidate()
    {
        // Apply inspector changes in editor and play mode
        if (timerText != null)
        {
            timerText.fontSize = timerFontSize;
            var timerRect = timerText.rectTransform;
            timerRect.offsetMin = textPadding;
            timerRect.offsetMax = -textPadding;
        }
        
        if (progressText != null)
        {
            progressText.fontSize = progressFontSize;
            var progressRect = progressText.rectTransform;
            progressRect.offsetMin = textPadding;
            progressRect.offsetMax = -textPadding;
        }
        
        if (challengeMessageText != null)
        {
            challengeMessageText.fontSize = messageFontSize;
            var messageRect = challengeMessageText.rectTransform;
            messageRect.offsetMin = textPadding;
            messageRect.offsetMax = -textPadding;
        }
        
        if (uiCanvas != null)
        {
            var rectTransform = uiCanvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = canvasSize;
            uiCanvas.transform.localScale = Vector3.one * canvasScale;
        }
        
        LoadMagicalFonts();
    }
    
    /// <summary>
    /// Creates a rounded rectangle sprite (placeholder)
    /// </summary>
    private Sprite CreateRoundedRectSprite()
    {
        // Return default UI sprite for now
        // In a full implementation, you'd create or load a rounded rectangle texture
        return null;
    }
    
    /// <summary>
    /// Creates a magical border sprite (placeholder)
    /// </summary>
    private Sprite CreateBorderSprite()
    {
        // Return default UI sprite for now
        // In a full implementation, you'd create or load a decorative border texture
        return null;
    }
    
    /// <summary>
    /// Follows camera with gentle magical floating motion
    /// </summary>
    private void FollowCameraWithMagicalMotion()
    {
        Vector3 baseTargetPosition = GetTargetPosition();
        
        // Add gentle floating motion
        float floatOffset = Mathf.Sin(Time.time * shimmerSpeed) * floatAmount;
        Vector3 magicalTargetPosition = baseTargetPosition + Vector3.up * floatOffset;
        
        // Smooth magical following
        if (followSpeed > 0f)
        {
            uiCanvas.transform.position = Vector3.Lerp(
                uiCanvas.transform.position, 
                magicalTargetPosition, 
                Time.deltaTime * followSpeed
            );
            
            if (lookAtCamera)
            {
                Vector3 lookDirection = uiCanvas.transform.position - playerCamera.transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                uiCanvas.transform.rotation = Quaternion.Slerp(
                    uiCanvas.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * followSpeed
                );
            }
        }
        else
        {
            uiCanvas.transform.position = magicalTargetPosition;
            
            if (lookAtCamera)
            {
                Vector3 lookDirection = uiCanvas.transform.position - playerCamera.transform.position;
                uiCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    /// <summary>
    /// Gets the target position for the UI relative to the camera
    /// </summary>
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
    /// Called when timer updates during a challenge
    /// </summary>
    private void OnTimerUpdate(float timeRemaining)
    {
        UpdateTimer(timeRemaining);
    }
    
    /// <summary>
    /// Called when a new magical challenge starts
    /// </summary>
    private void OnChallengeStarted(int itemCount, int timeLimit)
    {
        ShowUI();
        
        // Set initial magical values
        UpdateProgress(0, itemCount);
        // Timer will be updated through OnTimerUpdate events
        
        // Choose magical message based on item type
        string treasureType = (Random.value > 0.5f) ? "Chocolate Frogs" : "Golden Dragon Eggs";
        challengeMessageText.text = $"Seek the {itemCount} {treasureType}...";
        
        // Start magical effects
        StartMagicalEffects();
        
        Debug.Log($"[CollectibleUI] Magical challenge started: {itemCount} treasures, {timeLimit} seconds");
    }
    
    /// <summary>
    /// Called when a magical treasure is collected
    /// </summary>
    private void OnItemCollected(int collected, int total)
    {
        UpdateProgress(collected, total);
        
        // Magical collection messages
        string[] collectMessages = {
            "Excellent! The magic grows stronger...",
            "Well done! More treasures await...",
            "Brilliant! Your magical prowess shows...",
            "Splendid! The collection continues..."
        };
        
        if (collected < total)
        {
            string message = collectMessages[Random.Range(0, collectMessages.Length)];
            challengeMessageText.text = message;
            
            // Trigger collection sparkle effect
            StartCoroutine(CollectionSparkleEffect());
        }
        else
        {
            challengeMessageText.text = "All treasures found! Magnificent!";
        }
        
        Debug.Log($"[CollectibleUI] Magical treasure collected: {collected}/{total}");
    }
    
    /// <summary>
    /// Called when the magical challenge is completed
    /// </summary>
    private void OnChallengeCompleted(bool success)
    {
        StopMagicalEffects();
        
        if (success)
        {
            // Success magical messages
            string[] successMessages = {
                "Extraordinary! You've mastered the challenge!",
                "Brilliant magical collection! Well done!",
                "Magnificent! Your skills are impressive!",
                "Outstanding! A true treasure hunter!"
            };
            challengeMessageText.text = successMessages[Random.Range(0, successMessages.Length)];
            challengeMessageText.color = successEmerald;
            
            StartCoroutine(SuccessShimmerEffect());
        }
        else
        {
            // Failure magical messages
            string[] failureMessages = {
                "The magic fades... better luck next time.",
                "Time runs short... the treasures vanish.",
                "The challenge proves difficult... try again.",
                "Practice makes perfect... keep trying!"
            };
            challengeMessageText.text = failureMessages[Random.Range(0, failureMessages.Length)];
            challengeMessageText.color = criticalCrimson;
        }
        
        // Hide UI after delay
        StartCoroutine(HideUIDelayed(3f));
        
        Debug.Log($"[CollectibleUI] Magical challenge completed - Success: {success}");
    }
    
    /// <summary>
    /// Updates the magical timer display with appropriate coloring
    /// </summary>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) 
        {
            Debug.LogError("[CollectibleUI] Timer text is null! UI may not be properly initialized.");
            return;
        }
        
        // Format time elegantly
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes}:{seconds:00}";
        
        // Magical color transitions based on time
        if (timeRemaining <= criticalTimeThreshold)
        {
            timerText.color = criticalCrimson;
            // Add urgent pulsing effect
            if (magicalEffectsCoroutine == null)
            {
                magicalEffectsCoroutine = StartCoroutine(UrgentPulseEffect());
            }
        }
        else if (timeRemaining <= warningTimeThreshold)
        {
            timerText.color = warningAmber;
            StopUrgentEffects();
        }
        else
        {
            timerText.color = enchantedSilver;
            StopUrgentEffects();
        }
    }
    
    /// <summary>
    /// Updates the magical progress display
    /// </summary>
    public void UpdateProgress(int collected, int total)
    {
        if (progressText == null || progressBar == null) return;
        
        // Update magical progress text
        progressText.text = $"{collected} of {total} Treasures Found";
        
        // Update mystical progress bar
        float progress = (float)collected / total;
        progressBar.fillAmount = progress;
        
        // Color transition based on progress
        if (progress >= 1f)
        {
            progressBar.color = magicalGold;
        }
        else if (progress >= 0.5f)
        {
            progressBar.color = successEmerald;
        }
        else
        {
            progressBar.color = new Color(successEmerald.r, successEmerald.g, successEmerald.b, 0.7f);
        }
    }
    
    /// <summary>
    /// Shows the magical UI with fade-in effect
    /// </summary>
    public void ShowUI()
    {
        if (challengePanel != null)
        {
            challengePanel.SetActive(true);
            isVisible = true;
            StartCoroutine(FadeInEffect());
            Debug.Log("[CollectibleUI] UI shown successfully");
        }
        else
        {
            Debug.LogError("[CollectibleUI] Challenge panel is null! UI cannot be shown.");
        }
    }
    
    /// <summary>
    /// Hides the magical UI with fade-out effect
    /// </summary>
    public void HideUI()
    {
        if (challengePanel != null)
        {
            StartCoroutine(FadeOutEffect());
            isVisible = false;
        }
    }
    
    /// <summary>
    /// Starts magical visual effects
    /// </summary>
    private void StartMagicalEffects()
    {
        if (shimmerCoroutine == null)
        {
            shimmerCoroutine = StartCoroutine(BorderShimmerEffect());
        }
    }
    
    /// <summary>
    /// Stops magical visual effects
    /// </summary>
    private void StopMagicalEffects()
    {
        if (shimmerCoroutine != null)
        {
            StopCoroutine(shimmerCoroutine);
            shimmerCoroutine = null;
        }
        StopUrgentEffects();
    }
    
    /// <summary>
    /// Stops urgent timer effects
    /// </summary>
    private void StopUrgentEffects()
    {
        if (magicalEffectsCoroutine != null)
        {
            StopCoroutine(magicalEffectsCoroutine);
            magicalEffectsCoroutine = null;
        }
    }
    
    /// <summary>
    /// Magical border shimmer effect
    /// </summary>
    private IEnumerator BorderShimmerEffect()
    {
        while (isVisible && magicalBorder != null)
        {
            float shimmer = shimmerCurve.Evaluate((Mathf.Sin(Time.time * shimmerSpeed) + 1f) * 0.5f);
            Color shimmerColor = Color.Lerp(magicalGold, enchantedSilver, shimmer);
            magicalBorder.color = shimmerColor;
            yield return null;
        }
    }
    
    /// <summary>
    /// Urgent pulsing effect for critical time
    /// </summary>
    private IEnumerator UrgentPulseEffect()
    {
        while (timerText != null && isVisible)
        {
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            timerText.color = Color.Lerp(criticalCrimson, Color.white, pulse * 0.3f);
            yield return null;
        }
    }
    
    /// <summary>
    /// Success shimmer effect
    /// </summary>
    private IEnumerator SuccessShimmerEffect()
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration && challengeMessageText != null)
        {
            float shimmer = Mathf.Sin(elapsed * 10f) * 0.5f + 0.5f;
            Color shimmerColor = Color.Lerp(successEmerald, magicalGold, shimmer);
            challengeMessageText.color = shimmerColor;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// Collection sparkle effect
    /// </summary>
    private IEnumerator CollectionSparkleEffect()
    {
        // Brief scale and glow effect on progress text
        Vector3 originalScale = progressText.transform.localScale;
        Color originalColor = progressText.color;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
            float glow = Mathf.Sin(t * Mathf.PI);
            
            progressText.transform.localScale = originalScale * scale;
            progressText.color = Color.Lerp(originalColor, Color.white, glow * 0.3f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        progressText.transform.localScale = originalScale;
        progressText.color = originalColor;
    }
    
    /// <summary>
    /// Gentle fade-in effect
    /// </summary>
    private IEnumerator FadeInEffect()
    {
        CanvasGroup canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        float duration = 1f;
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Gentle fade-out effect
    /// </summary>
    private IEnumerator FadeOutEffect()
    {
        CanvasGroup canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            challengePanel.SetActive(false);
            yield break;
        }
        
        float duration = 1f;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        challengePanel.SetActive(false);
    }
    
    /// <summary>
    /// Hides UI after a magical delay
    /// </summary>
    private IEnumerator HideUIDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideUI();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (CollectibleSpawner.Instance != null)
        {
            CollectibleSpawner.Instance.OnChallengeStarted -= OnChallengeStarted;
            CollectibleSpawner.Instance.OnItemCollected -= OnItemCollected;
            CollectibleSpawner.Instance.OnChallengeCompleted -= OnChallengeCompleted;
            CollectibleSpawner.Instance.OnTimerUpdate -= OnTimerUpdate;
        }
        
        // Clean up coroutines
        StopMagicalEffects();
    }
} 