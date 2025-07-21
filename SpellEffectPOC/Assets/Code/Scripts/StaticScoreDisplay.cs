using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StaticScoreDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private Vector3 worldPosition = new Vector3(0f, 3f, 5f); // Fixed position in world
    [SerializeField] private Vector3 worldRotation = new Vector3(0f, 180f, 0f); // Face towards spawn area
    [SerializeField] private float canvasScale = 0.01f; // Larger scale for visibility
    [SerializeField] private Vector2 canvasSize = new Vector2(800f, 200f); // Big, wide scoreboard
    
    [Header("Visual Design")]
    [SerializeField] private Color scoreTextColor = new Color(1f, 0.84f, 0f, 1f); // Golden color
    [SerializeField] private Color backgroundColorNormal = new Color(0.1f, 0.05f, 0.2f, 0.9f); // Dark magical purple
    [SerializeField] private Color backgroundColorHighlight = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green for score increase
    [SerializeField] private Color borderColor = new Color(0.8f, 0.6f, 0.2f, 1f); // Golden border
    [SerializeField] private float borderWidth = 8f;
    
    [Header("Animation")]
    [SerializeField] private float highlightDuration = 1f;
    [SerializeField] private AnimationCurve highlightCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField] private float scoreUpdateAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve scoreScaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.3f, 1.2f), new Keyframe(1, 1));
    
    [Header("Audio")]
    [SerializeField] private AudioClip scoreUpdateSound;
    [SerializeField] private AudioSource audioSource;
    
    // UI Components
    private Canvas scoreCanvas;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI titleText;
    private Image backgroundImage;
    private Image borderImage;
    
    // Animation
    private Coroutine highlightCoroutine;
    private Coroutine scoreAnimationCoroutine;
    private int currentDisplayedScore = 0;
    private int targetScore = 0;
    
    // Singleton for easy access
    public static StaticScoreDisplay Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[StaticScoreDisplay] Multiple instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create the static score display
        CreateStaticScoreDisplay();
        
        // Subscribe to ScoreManager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            // Set initial score
            targetScore = ScoreManager.Instance.GetCurrentScore();
            currentDisplayedScore = targetScore;
            UpdateScoreText();
        }
    }
    
    void Start()
    {
        // If ScoreManager wasn't ready in Awake, try again
        if (ScoreManager.Instance != null && targetScore == 0)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            targetScore = ScoreManager.Instance.GetCurrentScore();
            currentDisplayedScore = targetScore;
            UpdateScoreText();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }
    }
    
    void CreateStaticScoreDisplay()
    {
        // Create main canvas GameObject
        GameObject canvasObject = new GameObject("StaticScoreCanvas");
        canvasObject.transform.SetParent(transform);
        
        // Position at fixed world coordinates
        canvasObject.transform.position = worldPosition;
        canvasObject.transform.rotation = Quaternion.Euler(worldRotation);
        canvasObject.transform.localScale = Vector3.one * canvasScale;
        
        // Setup Canvas component
        scoreCanvas = canvasObject.AddComponent<Canvas>();
        scoreCanvas.renderMode = RenderMode.WorldSpace;
        scoreCanvas.sortingOrder = 10;
        
        // Set canvas size
        var rectTransform = canvasObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = canvasSize;
        
        // Add CanvasScaler and GraphicRaycaster
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        canvasObject.AddComponent<GraphicRaycaster>();
        
        // Create the UI elements
        CreateScoreboardUI();
        
        Debug.Log($"[StaticScoreDisplay] Created static scoreboard at position: {worldPosition}");
    }
    
    void CreateScoreboardUI()
    {
        // Main container
        GameObject container = new GameObject("ScoreboardContainer");
        container.transform.SetParent(scoreCanvas.transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Border (outer frame)
        GameObject borderObject = new GameObject("Border");
        borderObject.transform.SetParent(container.transform, false);
        borderImage = borderObject.AddComponent<Image>();
        borderImage.color = borderColor;
        
        RectTransform borderRect = borderImage.rectTransform;
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        // Background (inner area)
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(container.transform, false);
        backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = backgroundColorNormal;
        
        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.one * borderWidth;
        backgroundRect.offsetMax = Vector2.one * -borderWidth;
        
        // Title Text
        GameObject titleObject = new GameObject("TitleText");
        titleObject.transform.SetParent(container.transform, false);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = "SCORE";
        titleText.fontSize = 48f;
        titleText.color = scoreTextColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.6f);
        titleRect.anchorMax = new Vector2(1f, 0.9f);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);
        
        // Score Text
        GameObject scoreObject = new GameObject("ScoreText");
        scoreObject.transform.SetParent(container.transform, false);
        scoreText = scoreObject.AddComponent<TextMeshProUGUI>();
        scoreText.text = "0";
        scoreText.fontSize = 72f;
        scoreText.color = scoreTextColor;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(0f, 0.1f);
        scoreRect.anchorMax = new Vector2(1f, 0.6f);
        scoreRect.offsetMin = new Vector2(20, 0);
        scoreRect.offsetMax = new Vector2(-20, 0);
        
        // Try to load magical font
        LoadMagicalFont();
        
        Debug.Log("[StaticScoreDisplay] Created scoreboard UI");
    }
    
    void LoadMagicalFont()
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
        
        if (magicalFont != null)
        {
            if (titleText != null) titleText.font = magicalFont;
            if (scoreText != null) scoreText.font = magicalFont;
            Debug.Log("[StaticScoreDisplay] Loaded magical font: " + magicalFont.name);
        }
        else
        {
            Debug.LogWarning("[StaticScoreDisplay] No magical font found. Using default font.");
        }
    }
    
    void OnScoreChanged(int newScore)
    {
        targetScore = newScore;
        
        // Play sound effect
        if (scoreUpdateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreUpdateSound);
        }
        
        // Trigger animations
        TriggerHighlightEffect();
        TriggerScoreAnimation();
        
        Debug.Log($"[StaticScoreDisplay] Score changed to: {newScore}");
    }
    
    void TriggerHighlightEffect()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }
        highlightCoroutine = StartCoroutine(HighlightEffectCoroutine());
    }
    
    void TriggerScoreAnimation()
    {
        if (scoreAnimationCoroutine != null)
        {
            StopCoroutine(scoreAnimationCoroutine);
        }
        scoreAnimationCoroutine = StartCoroutine(ScoreAnimationCoroutine());
    }
    
    IEnumerator HighlightEffectCoroutine()
    {
        if (backgroundImage == null) yield break;
        
        Color originalColor = backgroundColorNormal;
        Color highlightColor = backgroundColorHighlight;
        
        float elapsedTime = 0f;
        while (elapsedTime < highlightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / highlightDuration;
            float intensity = highlightCurve.Evaluate(t);
            
            backgroundImage.color = Color.Lerp(originalColor, highlightColor, intensity);
            
            yield return null;
        }
        
        // Restore original color
        backgroundImage.color = originalColor;
        highlightCoroutine = null;
    }
    
    IEnumerator ScoreAnimationCoroutine()
    {
        if (scoreText == null) yield break;
        
        Vector3 originalScale = scoreText.transform.localScale;
        int startScore = currentDisplayedScore;
        
        float elapsedTime = 0f;
        while (elapsedTime < scoreUpdateAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scoreUpdateAnimationDuration;
            
            // Animate scale
            float scaleMultiplier = scoreScaleCurve.Evaluate(t);
            scoreText.transform.localScale = originalScale * scaleMultiplier;
            
            // Animate score value
            currentDisplayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
            UpdateScoreText();
            
            yield return null;
        }
        
        // Ensure final values
        scoreText.transform.localScale = originalScale;
        currentDisplayedScore = targetScore;
        UpdateScoreText();
        
        scoreAnimationCoroutine = null;
    }
    
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = currentDisplayedScore.ToString();
        }
    }
    
    // Public methods for positioning
    public void SetWorldPosition(Vector3 position)
    {
        worldPosition = position;
        if (scoreCanvas != null)
        {
            scoreCanvas.transform.position = position;
        }
    }
    
    public void SetWorldRotation(Vector3 rotation)
    {
        worldRotation = rotation;
        if (scoreCanvas != null)
        {
            scoreCanvas.transform.rotation = Quaternion.Euler(rotation);
        }
    }
    
    // Static methods for easy access
    public static void SetStaticPosition(Vector3 position)
    {
        if (Instance != null)
        {
            Instance.SetWorldPosition(position);
        }
    }
    
    public static void SetStaticRotation(Vector3 rotation)
    {
        if (Instance != null)
        {
            Instance.SetWorldRotation(rotation);
        }
    }
} 