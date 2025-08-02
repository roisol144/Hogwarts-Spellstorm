using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private float countdownTime = 5f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Camera-Fixed UI Settings")]
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(0f, 0f, 1.2f); // Distance from camera (only Z is used)
    [SerializeField] private float canvasScale = 0.003f; // Legacy field - no longer used for screen space canvas
    [SerializeField] private Vector2 canvasSize = new Vector2(600f, 400f); // Canvas panel size in pixels
    
    [Header("UI Colors")]
    [SerializeField] private Color backgroundColorDark = new Color(0f, 0f, 0f, 0.8f); // Dark background
    [SerializeField] private Color titleColor = new Color(1f, 0.2f, 0.2f, 1f); // Red for "GAME OVER"
    [SerializeField] private Color victoryTitleColor = new Color(0.2f, 1f, 0.2f, 1f); // Green for "VICTORY!"
    [SerializeField] private Color scoreColor = new Color(1f, 0.84f, 0f, 1f); // Golden for score
    [SerializeField] private Color timerColor = new Color(1f, 1f, 1f, 1f); // White for timer
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private Canvas gameOverCanvas;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timerText;
    private Camera playerCamera;
    private PlayerHealth playerHealth;
    private bool isGameOver = false;
    private bool isVictory = false;
    private Coroutine countdownCoroutine;
    
    private void Awake()
    {
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
        
        // Find PlayerHealth component
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Subscribe to player death event
            playerHealth.OnPlayerDied += OnPlayerDied;
            Debug.Log("[GameOverUI] Subscribed to PlayerHealth.OnPlayerDied event");
        }
        else
        {
            Debug.LogWarning("[GameOverUI] PlayerHealth component not found! Game over UI won't trigger.");
        }
        
        // Subscribe to victory event
        GameLevelManager.OnVictoryAchieved += OnVictoryAchieved;
        Debug.Log("[GameOverUI] Subscribed to GameLevelManager.OnVictoryAchieved event");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= OnPlayerDied;
        }
        
        GameLevelManager.OnVictoryAchieved -= OnVictoryAchieved;
        
        // Stop any running coroutines
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }
    
    private void OnPlayerDied()
    {
        if (isGameOver) return; // Prevent multiple triggers
        
        isGameOver = true;
        Debug.Log("[GameOverUI] Player died! Showing game over screen...");
        
        // Play game over sound
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Create and show game over UI
        CreateGameOverUI();
        
        // Start countdown timer
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }
    
    private void OnVictoryAchieved(int finalScore)
    {
        if (isGameOver) return; // Prevent multiple triggers
        
        isGameOver = true;
        isVictory = true;
        Debug.Log($"[GameOverUI] Victory achieved with {finalScore} points! Showing victory screen...");
        
        // Play victory sound
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // Create and show victory UI
        CreateGameOverUI();
        
        // Start countdown timer
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }
    
    private void CreateGameOverUI()
    {
        // Create game over canvas (Camera Space - fixed to camera)
        GameObject canvasGO = new GameObject("GameOverCanvas_CameraSpace");
        gameOverCanvas = canvasGO.AddComponent<Canvas>();
        
        // Configure as Screen Space - Camera Canvas (automatically follows camera)
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        gameOverCanvas.worldCamera = playerCamera;
        gameOverCanvas.sortingOrder = 200; // Higher than other UI elements
        
        // Set canvas distance from camera (using Z offset from original settings)
        gameOverCanvas.planeDistance = offsetFromCamera.z;
        
        // Add CanvasScaler with original sizing approach for smaller panel
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 0.5f; // Scale factor to make it smaller like the original
        
        // Canvas size is no longer needed since we use a fixed-size background panel
        // The background panel will be 1200x700 and centered on the canvas
        
        // Add GraphicRaycaster for UI interactions (required for screen space canvas)
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create UI elements
        CreateGameOverElements();
        
        Debug.Log($"[GameOverUI] Created Game Over UI fixed to camera at distance: {gameOverCanvas.planeDistance}");
    }
    
    private void CreateGameOverElements()
    {
        // Background panel
        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(gameOverCanvas.transform, false);
        
        Image backgroundImage = backgroundPanel.AddComponent<Image>();
        backgroundImage.color = backgroundColorDark;
        
        RectTransform backgroundRect = backgroundImage.rectTransform;
        // Center the panel instead of stretching it to fill the screen
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(1200f, 700f); // Larger panel size for better visibility
        
        // Title Text: "GAME OVER" or "VICTORY!"
        GameObject titleObject = new GameObject("TitleText");
        titleObject.transform.SetParent(backgroundPanel.transform, false);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        
        if (isVictory)
        {
            titleText.text = "VICTORY!";
            titleText.color = victoryTitleColor;
        }
        else
        {
            titleText.text = "GAME OVER";
            titleText.color = titleColor;
        }
        
        titleText.fontSize = 80f; // Increased from 64f
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.7f);
        titleRect.anchorMax = new Vector2(1f, 0.9f);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);
        
        // Score Text
        GameObject scoreObject = new GameObject("ScoreText");
        scoreObject.transform.SetParent(backgroundPanel.transform, false);
        scoreText = scoreObject.AddComponent<TextMeshProUGUI>();
        
        // Get final score from ScoreManager
        int finalScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.GetCurrentScore();
        }
        
        if (isVictory)
        {
            string levelName = GameLevelManager.Instance != null ? GameLevelManager.Instance.CurrentLevelName : "Unknown";
            int winScore = GameLevelManager.Instance != null ? GameLevelManager.Instance.CurrentWinScore : 0;
            scoreText.text = $"🎉 {levelName} Level Completed! 🎉\nFinal Score: {finalScore} / {winScore}";
        }
        else
        {
            scoreText.text = $"Final Score: {finalScore}";
        }
        scoreText.fontSize = 60f; // Increased from 48f
        scoreText.color = scoreColor;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(0f, 0.4f);
        scoreRect.anchorMax = new Vector2(1f, 0.6f);
        scoreRect.offsetMin = new Vector2(20, 0);
        scoreRect.offsetMax = new Vector2(-20, 0);
        
        // Timer Text
        GameObject timerObject = new GameObject("TimerText");
        timerObject.transform.SetParent(backgroundPanel.transform, false);
        timerText = timerObject.AddComponent<TextMeshProUGUI>();
        timerText.text = $"Returning to Main Menu in {countdownTime:F0}s";
        timerText.fontSize = 40f; // Increased from 32f
        timerText.color = timerColor;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontStyle = FontStyles.Normal;
        
        RectTransform timerRect = timerText.rectTransform;
        timerRect.anchorMin = new Vector2(0f, 0.1f);
        timerRect.anchorMax = new Vector2(1f, 0.3f);
        timerRect.offsetMin = new Vector2(20, 0);
        timerRect.offsetMax = new Vector2(-20, 0);
        
        // Try to load magical font for all text elements
        LoadMagicalFont();
        
        Debug.Log($"[GameOverUI] Created game over UI with final score: {finalScore}");
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
        
        if (magicalFont != null)
        {
            if (titleText != null) titleText.font = magicalFont;
            if (scoreText != null) scoreText.font = magicalFont;
            if (timerText != null) timerText.font = magicalFont;
            Debug.Log("[GameOverUI] Loaded magical font: " + magicalFont.name);
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No magical font found. Using default font.");
        }
    }
    
    private IEnumerator CountdownCoroutine()
    {
        float timeRemaining = countdownTime;
        
        while (timeRemaining > 0f)
        {
            // Update timer text
            if (timerText != null)
            {
                timerText.text = $"Returning to Main Menu in {Mathf.Ceil(timeRemaining):F0}s";
            }
            
            yield return new WaitForSeconds(0.1f); // Update every 0.1 seconds for smooth countdown
            timeRemaining -= 0.1f;
        }
        
        // Time's up! Load main menu scene
        Debug.Log($"[GameOverUI] Countdown finished. Loading {mainMenuSceneName} scene...");
        LoadMainMenu();
    }
    
    private void LoadMainMenu()
    {
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log($"[GameOverUI] Loading scene: {mainMenuSceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameOverUI] Failed to load scene '{mainMenuSceneName}': {e.Message}");
            Debug.LogError($"[GameOverUI] Make sure the scene '{mainMenuSceneName}' is added to Build Settings!");
        }
    }
    
    // Public method to manually trigger game over (for testing)
    [ContextMenu("Test Game Over")]
    public void TestGameOver()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[GameOverUI] Test can only be run in Play mode");
            return;
        }
        
        Debug.Log("[GameOverUI] Testing game over UI...");
        OnPlayerDied();
    }
    
    // Public getter for countdown time (for testing/configuration)
    public float GetCountdownTime() => countdownTime;
} 