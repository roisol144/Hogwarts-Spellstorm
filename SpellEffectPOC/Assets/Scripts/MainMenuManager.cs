using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

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

        // Scoreboard Button - Show Scoreboard Popup
        if (scoreboardButton != null)
        {
            scoreboardButton.onClick.AddListener(() => {
                PlayButtonSound();
                ShowScoreboardPopup();
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

    #region Scene Management
    private void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
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
    private void ShowScoreboardPopup()
    {
        if (scoreboardPopup != null)
        {
            scoreboardPopup.SetActive(true);
            PopulateScoreboard();
        }
    }

    private void HideScoreboardPopup()
    {
        if (scoreboardPopup != null)
            scoreboardPopup.SetActive(false);
    }

    private void ShowSpellsBookPopup()
    {
        if (spellsBookPopup != null)
        {
            Debug.Log("Opening Spells Book Popup");
            
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
            // Position popup 2 meters in front of camera, at eye level
            Vector3 forward = mainCamera.transform.forward;
            Vector3 popupPosition = mainCamera.transform.position + forward * 2.0f;
            
            // Make sure popup faces the player
            popup.transform.position = popupPosition;
            popup.transform.LookAt(mainCamera.transform);
            popup.transform.Rotate(0, 180, 0); // Flip to face player correctly
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
        // Clear existing entries
        if (scoreboardContent != null)
        {
            foreach (Transform child in scoreboardContent)
            {
                Destroy(child.gameObject);
            }

            // Sample data - replace with actual database retrieval later
            string[] playerNames = { "Harry Potter", "Hermione Granger", "Ron Weasley", "Draco Malfoy", "Luna Lovegood" };
            int[] scores = { 2450, 2380, 2150, 1950, 1800 };

            for (int i = 0; i < playerNames.Length; i++)
            {
                CreateScoreEntry(i + 1, playerNames[i], scores[i]);
            }
        }
    }

    private void CreateScoreEntry(int rank, string playerName, int score)
    {
        if (scoreEntryPrefab != null && scoreboardContent != null)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scoreboardContent);
            
            // Find text components and set values
            TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entry.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

            if (rankText != null) rankText.text = rank.ToString();
            if (nameText != null) nameText.text = playerName;
            if (scoreText != null) scoreText.text = score.ToString();
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
        
        // Virtual keyboard no longer needed - Quest will handle it natively
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
        
        // Set a pre-defined name so user can just click Save & Play
        if (playerNameInput != null)
        {
            // Generate a random wizard name for testing
            string[] wizardNames = {
                "Harry Potter", "Hermione Granger", "Ron Weasley", 
                "Draco Malfoy", "Luna Lovegood", "Neville Longbottom",
                "Ginny Weasley", "Cedric Diggory", "Cho Chang", "Dean Thomas"
            };
            
            string randomName = wizardNames[Random.Range(0, wizardNames.Length)];
            playerNameInput.text = randomName;
            
            // Make it editable if user wants to change it (but not required)
            playerNameInput.interactable = true;
        }
    }
    
    private void SetActivePanel(GameObject activePanel)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);
        if (difficultySelectionPanel != null) difficultySelectionPanel.SetActive(false);
        if (playerNamePanel != null) playerNamePanel.SetActive(false);
        
        // Hide OVR keyboard when switching panels
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
        
        // Store player name for scoreboard
        PlayerPrefs.SetString("CurrentPlayerName", playerName);
        PlayerPrefs.Save();
        
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
} 