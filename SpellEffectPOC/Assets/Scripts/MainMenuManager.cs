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
    [SerializeField] public Button playButton;
    [SerializeField] public Button tutorialButton;
    [SerializeField] public Button scoreboardButton;
    [SerializeField] public Button spellsBookButton;
    [SerializeField] public Button quitButton;

    [Header("Popup Windows")]
    [SerializeField] private GameObject scoreboardPopup;
    [SerializeField] private GameObject spellsBookPopup;
    [SerializeField] public Button closeScoreboardButton;
    [SerializeField] public Button closeSpellsBookButton;

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
    [SerializeField] public Button dungeonsMapButton;
    [SerializeField] public Button chamberMapButton;
    [SerializeField] public Button mapBackButton;

    [Header("Difficulty Selection")]
    [SerializeField] public Button beginnerButton;
    [SerializeField] public Button intermediateButton;
    [SerializeField] public Button advancedButton;
    [SerializeField] public Button difficultyBackButton;

    [Header("Player Name")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] public Button saveAndPlayButton;
    [SerializeField] public Button nameBackButton;
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
    
    private void Update()
    {
        // Emergency close functionality is now handled by WorkingScoreboard
        // No Update method needed for scoreboard functionality
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

        // Scoreboard Button - Show Working Scoreboard
        if (scoreboardButton != null)
        {
            scoreboardButton.onClick.AddListener(() => {
                Debug.Log("SCOREBOARD BUTTON CLICKED!"); // Debug message
                PlayButtonSound();
                ShowWorkingScoreboard();
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

        // Close buttons for popups (scoreboard close is now handled by WorkingScoreboard)

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
    private void ShowWorkingScoreboard()
    {
        Debug.Log("ShowWorkingScoreboard() called");
        
        // Find or create the working scoreboard (now reads from file)
        WorkingScoreboard workingScoreboard = FindObjectOfType<WorkingScoreboard>();
        
        if (workingScoreboard == null)
        {
            Debug.Log("Creating working scoreboard...");
            GameObject scoreboardObj = new GameObject("WorkingScoreboardManager");
            workingScoreboard = scoreboardObj.AddComponent<WorkingScoreboard>();
            workingScoreboard.CreateWorkingScoreboard();
        }
        
        // Show the working scoreboard
        workingScoreboard.ShowScoreboard();
        
        Debug.Log("✅ Working scoreboard displayed!");
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
            
            // Note: Removed automatic positioning - spells book should stay in its Canvas position
            // This ensures it appears in the center like other menu panels
            
            spellsBookPopup.SetActive(true);
            LoadSpellsBookContent();
        }
        else
        {
            Debug.LogError("SpellsBookPopup is not assigned in the Inspector!");
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
                StartGameWithSettings();
            });
        }
        
        if (intermediateButton != null)
        {
            intermediateButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedDifficulty = 1;
                StartGameWithSettings();
            });
        }
        
        if (advancedButton != null)
        {
            advancedButton.onClick.AddListener(() => {
                PlayButtonSound();
                selectedDifficulty = 2;
                StartGameWithSettings();
            });
        }
        
        if (difficultyBackButton != null)
        {
            difficultyBackButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowMapSelection();
            });
        }
        
        // Note: Player name input UI elements are no longer used
        // since we skip directly from difficulty selection to game start
        // Keeping UI references for potential future use or manual testing
    }

    private void SetupKeyboardIntegration()
    {
        // Keyboard integration no longer needed since name input is skipped
        // Keeping method for backward compatibility
        Debug.Log("Keyboard integration skipped - name input no longer used");
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
        // This method is no longer used since we skip name input
        // Keeping for backward compatibility with UI references
        Debug.Log("ShowPlayerNameInput called but name input is disabled");

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
            
            // Note: Removed automatic positioning for all menu panels and popups
            // All UI elements should stay in their designed Canvas positions
            // This ensures consistent behavior across all menu screens
        }
    }
    
    private void OnSaveAndPlay()
    {
        // This method is no longer used since we skip name input
        // Keeping for backward compatibility with UI references
        StartGameWithSettings();
    }
    
    private void StartGameWithSettings()
    {
        // Set default player name since we're skipping name input
        playerName = "Player";
        
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

    #region Simple Positioning Methods
    // Simple positioning methods for remaining popups (not scoreboard)
    private void PositionSpellsBookInFrontOfPlayer(GameObject popup)
    {
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera != null && popup != null)
        {
            Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 2.5f;
            popup.transform.position = position;
            popup.transform.LookAt(mainCamera.transform);
            popup.transform.Rotate(0, 180, 0);
            popup.transform.localScale = Vector3.one * 0.8f;
        }
    }
    
    private void PositionPanelInFrontOfPlayer(GameObject panel)
    {
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera != null && panel != null)
        {
            Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 2.0f;
            panel.transform.position = position;
            panel.transform.LookAt(mainCamera.transform);
            panel.transform.Rotate(0, 180, 0);
            panel.transform.localScale = Vector3.one * 0.8f;
        }
    }
    #endregion
} 