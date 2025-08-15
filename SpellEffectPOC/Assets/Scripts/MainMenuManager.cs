using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button scoreboardButton;
    [SerializeField] private Button spellsBookButton;
    [SerializeField] private Button quitButton;

    [Header("Popup Windows")]
    [SerializeField] private GameObject scoreboardPopup;
    [SerializeField] private GameObject spellsBookPopup;
    [SerializeField] private Button closeScoreboardButton;
    [SerializeField] private Button closeSpellsBookButton;

    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioSource sfxSource;

    [Header("Scoreboard Content")]
    [SerializeField] private Transform scoreboardContent;
    [SerializeField] private GameObject scoreEntryPrefab;

    [Header("Spells Book Content")]
    [SerializeField] private Image spellBookImage;
    [SerializeField] private Sprite[] spellBookPages;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject difficultySelectionPanel;
    [SerializeField] private GameObject playerNamePanel;

    [Header("Map Selection")]
    [SerializeField] private Button dungeonsMapButton;
    [SerializeField] private Button chamberMapButton;
    [SerializeField] private Button mapBackButton;

    [Header("Difficulty Selection")]
    [SerializeField] private Button beginnerButton;
    [SerializeField] private Button intermediateButton;
    [SerializeField] private Button advancedButton;
    [SerializeField] private Button difficultyBackButton;

    [Header("Player Name")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button saveAndPlayButton;
    [SerializeField] private Button nameBackButton;
    [SerializeField] private GameObject ovrKeyboard;
    [SerializeField] private KeyboardInputManager keyboardInputManager;
    
    [Header("Scene Transition")]
    [SerializeField] private SceneTransitionManager sceneTransitionManager;

    // Game state storage
    private string selectedMap = "";
    private int selectedDifficulty = 0;
    private string playerName = "";

    private void Start()
    {
        InitializeButtons();
        SetupBackgroundMusic();
        InitializePopups();
        InitializeMenuFlow();
        SetupSceneTransition();
    }

    private void InitializeButtons()
    {
        // Play Button - Show Map Selection
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowMapSelection();
            });
        }

        // Tutorial Button - Load Tutorial Scene
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(() => {
                PlayButtonSound();
                LoadScene("TutorialScene");
            });
        }

        // Scoreboard Button - Show Scoreboard UI
        if (scoreboardButton != null)
        {
            scoreboardButton.onClick.AddListener(() => {
                Debug.Log("SCOREBOARD BUTTON CLICKED!"); // Debug message
                PlayButtonSound();
                ShowScoreboardUI();
            });
        }

        // Spells Book Button - Show Spells Book Popup
        if (spellsBookButton != null)
        {
            spellsBookButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowSpellsBookPopup();
            });
        }

        // Quit Button - Quit Application
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                PlayButtonSound();
                QuitGame();
            });
        }

        // Close buttons for popups
        if (closeScoreboardButton != null)
        {
            closeScoreboardButton.onClick.AddListener(() => {
                PlayButtonSound();
                HideScoreboardPopup();
            });
        }

        if (closeSpellsBookButton != null)
        {
            closeSpellsBookButton.onClick.AddListener(() => {
                PlayButtonSound();
                HideSpellsBookPopup();
            });
        }
    }

    private void SetupBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusic != null)
        {
            Debug.Log("Setting up background music: " + backgroundMusic.name);
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.volume = 0.5f; // Increased volume
            backgroundMusicSource.playOnAwake = true;
            backgroundMusicSource.spatialBlend = 0f; // 2D sound
            backgroundMusicSource.Play();
            
            if (backgroundMusicSource.isPlaying)
            {
                Debug.Log("Background music is playing successfully!");
            }
            else
            {
                Debug.LogError("Background music failed to play!");
            }
        }
        else
        {
            if (backgroundMusicSource == null)
                Debug.LogError("Background Music Source is not assigned!");
            if (backgroundMusic == null)
                Debug.LogError("Background Music AudioClip is not assigned!");
        }
    }

    private void InitializePopups()
    {
        // Hide popups initially
        if (scoreboardPopup != null)
            scoreboardPopup.SetActive(false);
            
        if (spellsBookPopup != null)
            spellsBookPopup.SetActive(false);
    }
    
    private void SetupSceneTransition()
    {
        // Auto-find SceneTransitionManager if not assigned
        if (sceneTransitionManager == null)
        {
            sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
            if (sceneTransitionManager != null)
            {
                Debug.Log("[MainMenuManager] Found SceneTransitionManager automatically");
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] No SceneTransitionManager found! Scene transitions will not have fade effects.");
            }
        }
    }

    #region Scene Management
    private void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        
        // Use scene transition manager if available for fade effect
        if (sceneTransitionManager != null)
        {
            // Get scene index by name
            int sceneIndex = GetSceneIndexByName(sceneName);
            if (sceneIndex >= 0)
            {
                sceneTransitionManager.GoToScene(sceneIndex);
            }
            else
            {
                Debug.LogWarning($"Scene '{sceneName}' not found in build settings, using direct load");
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            // Fallback to direct scene loading if no transition manager
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// Gets the scene index by scene name from build settings
    /// </summary>
    private int GetSceneIndexByName(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameFromPath.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        
        Debug.LogWarning($"Scene '{sceneName}' not found in build settings");
        return -1;
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    #region Popup Management
    private void ShowScoreboardUI()
    {
        Debug.Log("ShowScoreboardUI() called");
        Debug.Log($"scoreboardPopup is null: {scoreboardPopup == null}");
        
        // Use the existing simple scoreboard popup
        if (scoreboardPopup != null)
        {
            Debug.Log($"Scoreboard popup found: {scoreboardPopup.name}");
            
            // Hide the main menu when showing the scoreboard
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
                Debug.Log("Main menu hidden");
            }
            
            // Ensure scoreboard is active before positioning
            scoreboardPopup.SetActive(true);
            Debug.Log("Scoreboard popup activated");
            
            // Ensure proper Canvas configuration for VR
            EnsureScoreboardCanvasConfiguration(scoreboardPopup);
            
            // Position popup in front of player for VR AFTER activation and configuration
            PositionPopupInFrontOfPlayer(scoreboardPopup);
            
            // Force refresh the Canvas and UI elements
            Canvas scoreboardCanvas = scoreboardPopup.GetComponentInChildren<Canvas>();
            if (scoreboardCanvas != null)
            {
                scoreboardCanvas.enabled = false;
                scoreboardCanvas.enabled = true;
                Debug.Log("Scoreboard Canvas refreshed");
            }
            
            PopulateScoreboard(); // This now uses real saved scores!
            
            // Force UI layout rebuild to ensure proper sizing
            StartCoroutine(RefreshScoreboardLayout());
            
            Debug.Log("MainMenuManager: Opened scoreboard with real scores");
        }
        else
        {
            Debug.LogError("❌ SCOREBOARD POPUP IS NULL! Please assign scoreboardPopup in MainMenuManager Inspector!");
        }
    }
    
    private System.Collections.IEnumerator RefreshScoreboardLayout()
    {
        // Wait one frame for the UI to be properly initialized
        yield return null;
        
        if (scoreboardPopup != null)
        {
            // Force layout rebuild
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scoreboardPopup.GetComponent<RectTransform>());
            Debug.Log("Scoreboard layout rebuilt");
        }
    }
    
    /// <summary>
    /// Cleans up scoreboard visuals for better VR display
    /// </summary>
    private void CleanupScoreboardVisuals(GameObject popup)
    {
        if (popup == null) return;
        
        Debug.Log("🧹 Cleaning up scoreboard visuals...");
        
        // Fix text clarity issues
        TextMeshProUGUI[] allText = popup.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log($"Found {allText.Length} text components in scoreboard");
        
        foreach (var text in allText)
        {
            if (text != null)
            {
                // Ensure crisp text rendering
                text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f); // Full opacity
                text.fontMaterial.SetFloat("_FaceDilate", 0); // Crisp edges
                text.fontMaterial.SetFloat("_OutlineWidth", 0); // No outline interference
                
                // Force text mesh update
                text.ForceMeshUpdate();
                Debug.Log($"Cleaned text component: {text.name} - Color: {text.color}");
            }
        }
        
        // Make background transparent
        Image[] allImages = popup.GetComponentsInChildren<Image>();
        Debug.Log($"Found {allImages.Length} image components for background cleanup");
        
        foreach (var image in allImages)
        {
            if (image != null)
            {
                // Check if this is a background element (usually named "Background", "Panel", etc.)
                if (image.name.ToLower().Contains("background") || 
                    image.name.ToLower().Contains("panel") ||
                    image.transform == popup.transform) // Root level background
                {
                    // Make it fully transparent
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
                    Debug.Log($"Made background transparent: {image.name}");
                }
                else
                {
                    // For other images, ensure they're visible but check for duplicates
                    Debug.Log($"Image component: {image.name} - Color: {image.color}");
                }
            }
        }
        
        // Remove duplicate UI elements that might cause text layering
        CleanupDuplicateElements(popup);
    }
    
    /// <summary>
    /// Removes duplicate UI elements that could cause text layering
    /// </summary>
    private void CleanupDuplicateElements(GameObject popup)
    {
        // Look for scoreboardContent to clean up any duplicate entries
        if (scoreboardContent != null)
        {
            // Clear any existing entries before populating to prevent layering
            List<Transform> toDestroy = new List<Transform>();
            foreach (Transform child in scoreboardContent)
            {
                toDestroy.Add(child);
            }
            
            foreach (Transform child in toDestroy)
            {
                DestroyImmediate(child.gameObject);
            }
            
            Debug.Log($"Cleared {toDestroy.Count} existing scoreboard entries to prevent layering");
        }
    }
    
    /// <summary>
    /// Ensures the scoreboard has proper Canvas configuration for VR visibility
    /// </summary>
    private void EnsureScoreboardCanvasConfiguration(GameObject popup)
    {
        if (popup == null) return;
        
        Canvas canvas = popup.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = popup.GetComponentInChildren<Canvas>();
        }
        
        if (canvas != null)
        {
            // Ensure World Space rendering for VR
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;
            
            // Check and configure Canvas Scaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1.0f;
            
            // Ensure GraphicRaycaster for interactions
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            Debug.Log($"Canvas configuration verified for {popup.name}");
        }
        else
        {
            Debug.LogWarning($"No Canvas found on {popup.name} - creating one");
            
            // Create a new Canvas if none exists
            canvas = popup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;
            
            CanvasScaler scaler = popup.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1.0f;
            
            popup.AddComponent<GraphicRaycaster>();
            
            Debug.Log($"Created new Canvas for {popup.name}");
        }
    }

    private void HideScoreboardPopup()
    {
        if (scoreboardPopup != null)
        {
            scoreboardPopup.SetActive(false);
            
            // Show the main menu again when closing the scoreboard
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }
    }

    private void ShowSpellsBookPopup()
    {
        if (spellsBookPopup != null)
        {
            Debug.Log("Opening Spells Book Popup");
            
            // Hide the main menu when showing the spell book
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
            
            // Position popup in front of player
            PositionPopupInFrontOfPlayer(spellsBookPopup);
            
            spellsBookPopup.SetActive(true);
            LoadSpellsBookContent();
        }
        else
        {
            Debug.LogError("SpellsBookPopup is not assigned in the Inspector!");
        }
    }

    private void PositionPopupInFrontOfPlayer(GameObject popup)
    {
        // Find the main camera (player's head position)
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Try to find XR camera
            mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera != null)
        {
            // Enhanced positioning for better VR visibility
            Vector3 forward = mainCamera.transform.forward;
            Vector3 up = mainCamera.transform.up;
            Vector3 right = mainCamera.transform.right;
            
            // Position popup closer and at eye level for better visibility
            Vector3 popupPosition = mainCamera.transform.position + forward * 1.8f + up * 0.1f;
            
            // Set position and rotation
            popup.transform.position = popupPosition;
            popup.transform.LookAt(mainCamera.transform);
            popup.transform.Rotate(0, 180, 0); // Flip to face player correctly
            
            // Enhanced scaling for VR visibility with proper Canvas handling
            if (popup.name.Contains("Scoreboard"))
            {
                // Ensure the scoreboard is large enough to be readable in VR
                popup.transform.localScale = Vector3.one * 3.0f; // Even bigger scale for maximum VR visibility
                
                // Check if popup has a Canvas and adjust its properties for better VR display
                Canvas popupCanvas = popup.GetComponent<Canvas>();
                if (popupCanvas == null)
                {
                    popupCanvas = popup.GetComponentInChildren<Canvas>();
                }
                
                if (popupCanvas != null)
                {
                    // Ensure proper canvas settings for VR
                    popupCanvas.renderMode = RenderMode.WorldSpace;
                    popupCanvas.sortingOrder = 1000; // High sorting order to ensure visibility
                    
                    // Force the canvas to be world space and visible
                    popupCanvas.worldCamera = mainCamera;
                    
                    // Adjust canvas scaler for better readability
                    CanvasScaler scaler = popupCanvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                        scaler.scaleFactor = 1.0f;
                    }
                    
                    // Force immediate canvas update
                    Canvas.ForceUpdateCanvases();
                    
                    Debug.Log($"Scoreboard Canvas configured: RenderMode={popupCanvas.renderMode}, SortingOrder={popupCanvas.sortingOrder}, Scale={popup.transform.localScale}");
                }
                
                // Fix text clarity and background transparency
                CleanupScoreboardVisuals(popup);
            }
            else
            {
                popup.transform.localScale = Vector3.one * 0.8f; // Slightly larger scale for other popups
            }
            
            Debug.Log($"Positioned {popup.name} at: {popupPosition}, Scale: {popup.transform.localScale}");
        }
    }

    private void HideSpellsBookPopup()
    {
        if (spellsBookPopup != null)
        {
            Debug.Log("Closing Spells Book Popup");
            spellsBookPopup.SetActive(false);
            
            // Force refresh to ensure it's hidden
            spellsBookPopup.transform.gameObject.SetActive(false);
            
            // Show the main menu again when closing the spell book
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("SpellsBookPopup is not assigned in the Inspector!");
        }
    }
    #endregion

    #region Scoreboard Content
    private void PopulateScoreboard()
    {
        // Clear existing entries to prevent text layering
        if (scoreboardContent != null)
        {
            // Use DestroyImmediate to ensure immediate cleanup
            List<Transform> toDestroy = new List<Transform>();
            foreach (Transform child in scoreboardContent)
            {
                toDestroy.Add(child);
            }
            
            foreach (Transform child in toDestroy)
            {
                DestroyImmediate(child.gameObject);
            }
            
            Debug.Log($"Cleared {toDestroy.Count} existing scoreboard entries before populating");

            // Get real scores from GameScoreManager
            if (GameScoreManager.Instance != null)
            {
                var topScores = GameScoreManager.Instance.GetTopScores(10); // Get top 10 scores
                
                for (int i = 0; i < topScores.Count; i++)
                {
                    var scoreEntry = topScores[i];
                    CreateScoreEntry(i + 1, scoreEntry.playerName, scoreEntry.score, scoreEntry.mapName, scoreEntry.difficulty);
                }

                // If no scores exist, show sample data for testing
                if (topScores.Count == 0)
                {
                    CreateNoScoresMessage();
                }
            }
            else
            {
                // Fallback to sample data if score manager not available  
                string[] playerNames = { "Harry Potter", "Hermione Granger", "Ron Weasley", "Draco Malfoy", "Luna Lovegood", "Ginny Weasley", "Neville Longbottom", "Cedric Diggory", "Cho Chang", "Dean Thomas" };
                int[] scores = { 2450, 2380, 2150, 1950, 1800, 1650, 1500, 1350, 1200, 1050 };
                string[] maps = { "Dungeons", "Chamber", "Dungeons", "Chamber", "Dungeons", "Chamber", "Dungeons", "Chamber", "Dungeons", "Chamber" };
                string[] difficulties = { "Advanced", "Advanced", "Intermediate", "Advanced", "Beginner", "Intermediate", "Beginner", "Advanced", "Intermediate", "Beginner" };

                for (int i = 0; i < playerNames.Length; i++)
                {
                    CreateScoreEntry(i + 1, playerNames[i], scores[i], maps[i], difficulties[i]);
                }
            }
        }
    }

    private void CreateScoreEntry(int rank, string playerName, int score, string mapName = "", string difficulty = "")
    {
        if (scoreEntryPrefab != null && scoreboardContent != null)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreboardContent);
            
            // Find text components and set values (flexible approach for any prefab)
            TextMeshProUGUI[] textComponents = entry.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Try to find by name first, then fallback to order
            TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI mapText = entry.transform.Find("MapText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI difficultyText = entry.transform.Find("DifficultyText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entry.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

            // Set rank (Position 0: RankText)
            if (rankText != null) 
                rankText.text = rank.ToString();
            else if (textComponents.Length > 0) 
                textComponents[0].text = rank.ToString();

            // Set player name (Position 1: NameText)
            if (nameText != null) 
                nameText.text = playerName;
            else if (textComponents.Length > 1) 
                textComponents[1].text = playerName;

            // Set map (Position 2: MapText)
            if (mapText != null && !string.IsNullOrEmpty(mapName))
                mapText.text = GetMapDisplayName(mapName);
            else if (textComponents.Length > 2 && !string.IsNullOrEmpty(mapName))
                textComponents[2].text = GetMapDisplayName(mapName);

            // Set difficulty (Position 3: DifficultyText)
            if (difficultyText != null && !string.IsNullOrEmpty(difficulty))
                difficultyText.text = difficulty;
            else if (textComponents.Length > 3 && !string.IsNullOrEmpty(difficulty))
                textComponents[3].text = difficulty;

            // Set score (Position 4: ScoreText)
            if (scoreText != null) 
                scoreText.text = score.ToString("N0");
            else if (textComponents.Length > 4) 
                textComponents[4].text = score.ToString("N0");

            // Add gold/silver/bronze styling for top 3
            if (rank <= 3)
            {
                Image background = entry.GetComponent<Image>();
                if (background != null)
                {
                    switch (rank)
                    {
                        case 1: background.color = new Color(1f, 0.8f, 0f, 0.3f); break; // Gold
                        case 2: background.color = new Color(0.7f, 0.7f, 0.7f, 0.3f); break; // Silver
                        case 3: background.color = new Color(0.6f, 0.3f, 0f, 0.3f); break; // Bronze
                    }
                }
            }
        }
    }

    private void CreateNoScoresMessage()
    {
        if (scoreEntryPrefab != null && scoreboardContent != null)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreboardContent);
            
            // Set a "no scores" message
            TextMeshProUGUI[] textComponents = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponents.Length > 0)
            {
                textComponents[0].text = "No scores yet!";
                if (textComponents.Length > 1)
                    textComponents[1].text = "Start playing to see scores here";
                
                // Make text italic and gray
                foreach (var text in textComponents)
                {
                    text.fontStyle = FontStyles.Italic;
                    text.color = Color.gray;
                }
            }
        }
    }

    private string GetMapDisplayName(string mapName)
    {
        switch (mapName)
        {
            case "DungeonsScene": return "Dungeons";
            case "ChamberOfSecretsScene": return "Chamber";
            default: return mapName;
        }
    }
    #endregion

    #region Spells Book Content
    private void LoadSpellsBookContent()
    {
        if (spellBookImage != null && spellBookPages != null && spellBookPages.Length > 0)
        {
            // Load the first page by default
            spellBookImage.sprite = spellBookPages[0];
        }
    }
    #endregion

    #region Audio
    private void PlayButtonSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }
    #endregion

    #region Public Methods for External Access
    public void SetBackgroundMusicVolume(float volume)
    {
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = volume;
    }

    public void ToggleBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            if (backgroundMusicSource.isPlaying)
                backgroundMusicSource.Pause();
            else
                backgroundMusicSource.UnPause();
        }
    }
    #endregion

    #region Menu Flow System
    private void InitializeMenuFlow()
    {
        // Initialize panel states
        SetActivePanel(mainMenuPanel);
        
        // Setup map selection buttons
        if (dungeonsMapButton != null)
        {
            dungeonsMapButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedMap = "DungeonsScene";
                ShowDifficultySelection();
            });
        }
        
        if (chamberMapButton != null)
        {
            chamberMapButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedMap = "ChamberOfSecretsScene";
                ShowDifficultySelection();
            });
        }
        
        if (mapBackButton != null)
        {
            mapBackButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowMainMenu();
            });
        }
        
        // Setup difficulty selection buttons
        if (beginnerButton != null)
        {
            beginnerButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedDifficulty = 0;
                ShowPlayerNameInput();
            });
        }
        
        if (intermediateButton != null)
        {
            intermediateButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedDifficulty = 1;
                ShowPlayerNameInput();
            });
        }
        
        if (advancedButton != null)
        {
            advancedButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedDifficulty = 2;
                ShowPlayerNameInput();
            });
        }
        
        if (difficultyBackButton != null)
        {
            difficultyBackButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowMapSelection();
            });
        }
        
        // Setup player name buttons
        if (saveAndPlayButton != null)
        {
            saveAndPlayButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnSaveAndPlay();
            });
        }
        
        if (nameBackButton != null)
        {
            nameBackButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowDifficultySelection();
            });
        }
        
        // Setup keyboard integration
        SetupKeyboardIntegration();
    }

    private void SetupKeyboardIntegration()
    {
        // Initialize keyboard manager with the player name input field
        if (keyboardInputManager != null && playerNameInput != null)
        {
            keyboardInputManager.SetTargetInputField(playerNameInput);
            Debug.Log("Keyboard integration setup complete");
        }
        else
        {
            if (keyboardInputManager == null)
                Debug.LogWarning("KeyboardInputManager not assigned in MainMenuManager!");
            if (playerNameInput == null)
                Debug.LogWarning("Player Name Input Field not assigned in MainMenuManager!");
        }
    }
    
    private void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    private void ShowMapSelection()
    {
        SetActivePanel(mapSelectionPanel);
    }
    
    private void ShowDifficultySelection()
    {
        SetActivePanel(difficultySelectionPanel);
    }
    
    private void ShowPlayerNameInput()
    {
        SetActivePanel(playerNamePanel);
        
        // Keep the input field empty initially - let user type their own name
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text.Trim()))
        {
            Debug.Log($"Preserving existing player name: '{playerNameInput.text}'");
        }
        else if (playerNameInput != null)
        {
            // Keep field empty so user can enter their own name
            playerNameInput.text = "";
            Debug.Log("Player name field is ready for user input");
        }

        // Make sure input field is interactable
        if (playerNameInput != null)
        {
            playerNameInput.interactable = true;
        }

        // Show keyboard if configured to do so
        if (keyboardInputManager != null)
        {
            // Optional: Auto-show keyboard when panel opens
            // keyboardInputManager.ShowKeyboard();
        }
    }
    
    private void SetActivePanel(GameObject activePanel)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);
        if (difficultySelectionPanel != null) difficultySelectionPanel.SetActive(false);
        if (playerNamePanel != null) playerNamePanel.SetActive(false);
        
        // Hide keyboard when switching panels (except when going to player name panel)
        if (keyboardInputManager != null && activePanel != playerNamePanel)
        {
            keyboardInputManager.HideKeyboard();
        }
        
        // Also close MRTK keyboard directly when leaving player name panel
        if (activePanel != playerNamePanel && NonNativeKeyboard.Instance != null && 
            NonNativeKeyboard.Instance.gameObject.activeInHierarchy)
        {
            NonNativeKeyboard.Instance.Close();
            Debug.Log("MainMenuManager: Closed MRTK keyboard when switching panels");
        }
        
        // Also hide OVR keyboard if it exists
        if (ovrKeyboard != null && activePanel != playerNamePanel)
        {
            ovrKeyboard.SetActive(false);
        }
        
        // Show active panel
        if (activePanel != null)
        {
            activePanel.SetActive(true);
            
            // Position in front of player for VR (except main menu)
            if (activePanel != mainMenuPanel)
            {
                PositionPopupInFrontOfPlayer(activePanel);
            }
        }
    }
    
    private void OnSaveAndPlay()
    {
        // Validate player name
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text.Trim()))
        {
            playerName = playerNameInput.text.Trim();
            
            // Limit name length
            if (playerName.Length > 20)
            {
                playerName = playerName.Substring(0, 20);
            }
            
            StartGameWithSettings();
        }
        else
        {
            Debug.LogWarning("Player name is required!");
            // Optionally show a message to the player
            if (playerNameInput != null)
            {
                playerNameInput.ActivateInputField();
            }
        }
    }
    
    private void StartGameWithSettings()
    {
        Debug.Log($"Starting game - Map: {selectedMap}, Difficulty: {selectedDifficulty}, Player: {playerName}");
        
        // Initialize score system with current player and game settings
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.StartGameSession(playerName, selectedMap, selectedDifficulty.ToString());
        }
        else
        {
            Debug.LogWarning("GameScoreManager not found! Score tracking will not work.");
        }
        
        // Store player name for scoreboard
        PlayerPrefs.SetString("CurrentPlayerName", playerName);
        PlayerPrefs.Save();
        
        // Load the selected scene
        LoadScene(selectedMap);
        // Always store difficulty in PlayerPrefs for the game scene to load
        PlayerPrefs.SetInt("SelectedDifficulty", selectedDifficulty);
        Debug.Log($"[MainMenuManager] Stored difficulty {selectedDifficulty} ({GetDifficultyName(selectedDifficulty)}) in PlayerPrefs");
        
        // Also set on GameLevelManager if available (though it probably isn't in menu scene)
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.SetLevel(selectedDifficulty);
            Debug.Log("[MainMenuManager] Applied difficulty to existing GameLevelManager");
        }
        
        // Load selected map
        if (!string.IsNullOrEmpty(selectedMap))
        {
            LoadScene(selectedMap);
        }
        else
        {
            Debug.LogError("No map selected!");
        }
    }
    
    private string GetDifficultyName(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return "Beginner";
            case 1: return "Intermediate"; 
            case 2: return "Advanced";
            default: return "Unknown";
        }
    }
    #endregion
    
    #region Debug and Validation Methods
    /// <summary>
    /// Validates the scoreboard setup for VR visibility (call this from inspector or console)
    /// </summary>
    [ContextMenu("Validate Scoreboard Setup")]
    public void ValidateScoreboardSetup()
    {
        if (scoreboardPopup == null)
        {
            Debug.LogError("❌ Scoreboard popup is not assigned!");
            return;
        }
        
        Debug.Log("🔍 Validating Scoreboard Setup...");
        
        // Check Canvas configuration
        Canvas canvas = scoreboardPopup.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("⚠️ No Canvas found on scoreboard popup");
        }
        else
        {
            Debug.Log($"✅ Canvas found: RenderMode={canvas.renderMode}, SortingOrder={canvas.sortingOrder}");
        }
        
        // Check scale
        Debug.Log($"📏 Current Scale: {scoreboardPopup.transform.localScale}");
        
        // Check if it has score entry prefab
        if (scoreEntryPrefab == null)
        {
            Debug.LogWarning("⚠️ Score Entry Prefab not assigned");
        }
        else
        {
            Debug.Log("✅ Score Entry Prefab assigned");
        }
        
        // Check content container
        if (scoreboardContent == null)
        {
            Debug.LogWarning("⚠️ Scoreboard Content container not assigned");
        }
        else
        {
            Debug.Log("✅ Scoreboard Content container assigned");
        }
        
        Debug.Log("🎯 Scoreboard validation complete!");
    }
    #endregion
} 