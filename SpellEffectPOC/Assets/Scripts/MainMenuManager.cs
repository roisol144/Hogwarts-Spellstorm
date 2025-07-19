using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
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

    private void Start()
    {
        InitializeButtons();
        SetupBackgroundMusic();
        InitializePopups();
    }

    private void InitializeButtons()
    {
        // Play Button - Load YardScene
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => {
                PlayButtonSound();
                LoadScene("YardScene");
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
} 