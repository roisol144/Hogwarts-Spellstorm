using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// WORKING SCOREBOARD: Static positioning like main menu
/// Uses World Space Canvas positioned exactly where the main menu is
/// Static and comfortable for VR viewing
/// </summary>
public class WorkingScoreboard : MonoBehaviour
{
    [Header("🎯 WORKING Scoreboard - Static World Space")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool debugPositioning = true;
    
    [Header("📍 Manual Positioning Controls (Optional)")]
    [SerializeField] private bool useManualPositioning = false; // DISABLED by default - uses menu position
    [SerializeField] private Vector3 manualPosition = new Vector3(0f, 1.5f, 3f); // 3 units in front, 1.5m high
    [SerializeField] private Vector3 manualRotation = new Vector3(0f, 0f, 0f); // Face forward
    [SerializeField] private float manualScale = 0.001f; // Good scale for world space
    
    [Header("📋 Data Settings")]
    [SerializeField] private bool useRealGameData = true;
    [SerializeField] private string[] samplePlayerNames = { 
        "Harry Potter", "Hermione Granger", "Ron Weasley", "Draco Malfoy", "Luna Lovegood" 
    };
    [SerializeField] private int[] sampleScores = { 2450, 2380, 2150, 1950, 1800 };
    
    [Header("🎨 Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.15f, 0.95f);
    [SerializeField] private Color titleColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas scoreboardCanvas;
    private GameObject scoreboardPanel;
    private Camera playerCamera;
    private bool isVisible = false;
    
    private void Start()
    {
        if (setupOnStart)
        {
            CreateWorkingScoreboard();
            ConnectToScoreboardButton();
        }
    }
    
    [ContextMenu("🚀 Create Working Scoreboard")]
    public void CreateWorkingScoreboard()
    {
        Debug.Log("🚀 Creating WORKING scoreboard using Screen Space Camera...");
        
        // Find player camera
        FindPlayerCamera();
        
        // Create the scoreboard canvas
        CreateScoreboardCanvas();
        
        // Create the UI
        CreateScoreboardUI();
        
        // Hide initially
        HideScoreboard();
        
        Debug.Log("✅ Working scoreboard created successfully!");
    }
    
    private void FindPlayerCamera()
    {
        // Find the main camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (playerCamera != null)
        {
            Debug.Log($"✅ Found player camera: {playerCamera.name}");
        }
        else
        {
            Debug.LogError("❌ No camera found!");
        }
    }
    
    private void CreateScoreboardCanvas()
    {
        // Create canvas GameObject
        GameObject canvasObj = new GameObject("WorkingScoreboardCanvas");
        scoreboardCanvas = canvasObj.AddComponent<Canvas>();
        
        // Copy Canvas settings from working main menu for identical behavior
        GameObject mainMenu = FindMainMenu();
        if (mainMenu != null)
        {
            Canvas mainMenuCanvas = mainMenu.GetComponent<Canvas>();
            if (mainMenuCanvas != null)
            {
                // Copy exact Canvas settings that work in main menu
                scoreboardCanvas.renderMode = mainMenuCanvas.renderMode;
                scoreboardCanvas.worldCamera = mainMenuCanvas.worldCamera;
                scoreboardCanvas.sortingOrder = mainMenuCanvas.sortingOrder + 1; // Slightly higher
                scoreboardCanvas.overrideSorting = mainMenuCanvas.overrideSorting;
                scoreboardCanvas.sortingLayerID = mainMenuCanvas.sortingLayerID;
                
                Debug.Log($"✅ Copied Canvas settings from main menu:");
                Debug.Log($"   renderMode: {scoreboardCanvas.renderMode}");
                Debug.Log($"   worldCamera: {scoreboardCanvas.worldCamera?.name ?? "null"}");
                Debug.Log($"   sortingOrder: {scoreboardCanvas.sortingOrder}");
            }
            else
            {
                // Fallback to World Space
                scoreboardCanvas.renderMode = RenderMode.WorldSpace;
                scoreboardCanvas.worldCamera = playerCamera;
                scoreboardCanvas.sortingOrder = 100;
                Debug.Log("⚠️ Main menu has no Canvas - using fallback World Space settings");
            }
        }
        else
        {
            // Default World Space
            scoreboardCanvas.renderMode = RenderMode.WorldSpace;
            scoreboardCanvas.worldCamera = playerCamera;
            scoreboardCanvas.sortingOrder = 100;
            Debug.Log("⚠️ Main menu not found - using default World Space settings");
        }
        
        // Position it exactly where the main menu is
        PositionScoreboardLikeMenu(canvasObj);
        
        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1.0f;
        
        // Add GraphicRaycaster for VR interaction - copy settings from main menu
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        // Copy GraphicRaycaster settings from working main menu
        if (mainMenu != null)
        {
            GraphicRaycaster mainMenuRaycaster = mainMenu.GetComponent<GraphicRaycaster>();
            if (mainMenuRaycaster != null)
            {
                // Copy exact settings that work in main menu
                raycaster.ignoreReversedGraphics = mainMenuRaycaster.ignoreReversedGraphics;
                raycaster.blockingObjects = mainMenuRaycaster.blockingObjects;
                raycaster.blockingMask = mainMenuRaycaster.blockingMask;
                
                Debug.Log($"✅ Copied GraphicRaycaster settings from working main menu");
                Debug.Log($"   ignoreReversedGraphics: {raycaster.ignoreReversedGraphics}");
                Debug.Log($"   blockingObjects: {raycaster.blockingObjects}");
            }
            else
            {
                // Fallback VR settings
                raycaster.ignoreReversedGraphics = false;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                Debug.Log("⚠️ Main menu has no GraphicRaycaster - using fallback VR settings");
            }
        }
        else
        {
            // Default VR settings
            raycaster.ignoreReversedGraphics = false;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            Debug.Log("⚠️ Main menu not found - using default VR settings");
        }
        
        Debug.Log("✅ Created World Space canvas with VR-ready GraphicRaycaster");
    }
    
    private void PositionScoreboardLikeMenu(GameObject canvasObj)
    {
        if (useManualPositioning)
        {
            // Use manual positioning - much easier to control!
            if (playerCamera != null)
            {
                // Position relative to camera (or use absolute world position)
                Vector3 finalPosition = manualPosition;
                
                // If position is close to origin, make it relative to camera
                if (manualPosition.magnitude < 10f)
                {
                    finalPosition = playerCamera.transform.position + 
                                  playerCamera.transform.forward * manualPosition.z +
                                  playerCamera.transform.up * manualPosition.y +
                                  playerCamera.transform.right * manualPosition.x;
                }
                
                canvasObj.transform.position = finalPosition;
                canvasObj.transform.rotation = Quaternion.Euler(manualRotation);
                canvasObj.transform.localScale = Vector3.one * manualScale;
                
                if (debugPositioning)
                {
                    Debug.Log("🎯 Using MANUAL positioning for scoreboard");
                    Debug.Log($"📍 Position: {finalPosition}");
                    Debug.Log($"🔄 Rotation: {manualRotation}");
                    Debug.Log($"📏 Scale: {manualScale}");
                    Debug.Log($"📷 Camera Position: {playerCamera.transform.position}");
                }
            }
            else
            {
                // Use absolute manual position if no camera
                canvasObj.transform.position = manualPosition;
                canvasObj.transform.rotation = Quaternion.Euler(manualRotation);
                canvasObj.transform.localScale = Vector3.one * manualScale;
                
                Debug.Log("🎯 Using ABSOLUTE manual positioning (no camera found)");
            }
            return;
        }
        
        // Original logic: Copy main menu position
        GameObject mainMenu = FindMainMenu();
        
        if (mainMenu != null)
        {
            // Copy the exact position, rotation, and scale from main menu
            canvasObj.transform.position = mainMenu.transform.position;
            canvasObj.transform.rotation = mainMenu.transform.rotation;
            canvasObj.transform.localScale = mainMenu.transform.localScale;
            
            // Get the Canvas component to match render settings
            Canvas mainMenuCanvas = mainMenu.GetComponent<Canvas>();
            if (mainMenuCanvas != null && mainMenuCanvas.renderMode == RenderMode.WorldSpace)
            {
                // Copy any specific world space settings
                RectTransform mainMenuRect = mainMenu.GetComponent<RectTransform>();
                RectTransform scoreboardRect = canvasObj.GetComponent<RectTransform>();
                
                if (mainMenuRect != null && scoreboardRect != null)
                {
                    scoreboardRect.sizeDelta = mainMenuRect.sizeDelta;
                }
            }
            
            if (debugPositioning)
            {
                Debug.Log($"✅ Positioned scoreboard canvas to match main menu: {mainMenu.name}");
                Debug.Log($"📍 Position: {canvasObj.transform.position}");
                Debug.Log($"🔄 Rotation: {canvasObj.transform.rotation.eulerAngles}");
                Debug.Log($"📏 Scale: {canvasObj.transform.localScale}");
            }
        }
        else
        {
            // Fallback positioning if we can't find the main menu
            if (playerCamera != null)
            {
                Vector3 position = playerCamera.transform.position + playerCamera.transform.forward * 3f;
                canvasObj.transform.position = position;
                canvasObj.transform.LookAt(playerCamera.transform);
                canvasObj.transform.Rotate(0, 180, 0);
                canvasObj.transform.localScale = Vector3.one * 0.001f; // Small scale for world space
            }
            
            Debug.Log("⚠️ Main menu not found - using fallback positioning");
        }
    }
    
    private void CreateScoreboardUI()
    {
        // Main panel
        GameObject panelObj = new GameObject("ScoreboardPanel");
        panelObj.transform.SetParent(scoreboardCanvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.2f);
        panelRect.anchorMax = new Vector2(0.8f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Background
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = backgroundColor;
        
        scoreboardPanel = panelObj;
        
        // Title
        CreateTitle(panelObj);
        
        // Close button
        CreateCloseButton(panelObj);
        
        // Score entries
        CreateScoreEntries(panelObj);
        
        Debug.Log("✅ Created scoreboard UI");
    }
    
    private void CreateTitle(GameObject parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(10, 0);
        titleRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SCOREBOARD"; // Simple text without emojis for compatibility
        titleText.fontSize = 48;
        titleText.color = titleColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
    }
    
    private void CreateCloseButton(GameObject parent)
    {
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-25, -25);
        buttonRect.sizeDelta = new Vector2(40, 40);
        buttonRect.pivot = new Vector2(1, 1);
        
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(HideScoreboard);
        
        // X text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "X"; // Simple X instead of special character for compatibility
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
    }
    
    private void CreateScoreEntries(GameObject parent)
    {
        // Content area
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(parent.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 0.75f);
        contentRect.offsetMin = new Vector2(20, 20);
        contentRect.offsetMax = new Vector2(-20, -10);
        
        // Layout group
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        
        // This will be populated by RefreshScoreboardData
        // Initial placeholder - real data loaded when shown
    }
    
    private void CreateScoreEntry(GameObject parent, int rank, string playerName, int score)
    {
        GameObject entryObj = new GameObject($"Entry_{rank}");
        entryObj.transform.SetParent(parent.transform, false);
        
        RectTransform entryRect = entryObj.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 50);
        
        // Background
        Image entryBg = entryObj.AddComponent<Image>();
        Color bgColor = new Color(0.2f, 0.2f, 0.3f, 0.5f);
        
        // Medal colors for top 3
        if (rank == 1) bgColor = new Color(1f, 0.8f, 0f, 0.3f); // Gold
        else if (rank == 2) bgColor = new Color(0.7f, 0.7f, 0.7f, 0.3f); // Silver
        else if (rank == 3) bgColor = new Color(0.6f, 0.3f, 0f, 0.3f); // Bronze
        
        entryBg.color = bgColor;
        
        // Horizontal layout
        HorizontalLayoutGroup entryLayout = entryObj.AddComponent<HorizontalLayoutGroup>();
        entryLayout.padding = new RectOffset(15, 15, 10, 10);
        entryLayout.spacing = 20;
        entryLayout.childControlWidth = false;
        entryLayout.childControlHeight = true;
        
        // Rank text
        CreateEntryText(entryObj, $"#{rank}", 80, titleColor, TextAlignmentOptions.Center, FontStyles.Bold);
        
        // Name text
        CreateEntryText(entryObj, playerName, 300, textColor, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        
        // Score text
        CreateEntryText(entryObj, score.ToString("N0"), 150, titleColor, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        
        // Layout element
        LayoutElement element = entryObj.AddComponent<LayoutElement>();
        element.preferredHeight = 50;
    }
    
    private void CreateEntryText(GameObject parent, string text, float width, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 20;
        textComp.color = color;
        textComp.alignment = alignment;
        textComp.fontStyle = style;
        
        LayoutElement element = textObj.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.flexibleWidth = 0;
    }
    
    private void RefreshScoreboardData()
    {
        // Find the content container
        Transform contentParent = scoreboardCanvas.transform.Find("ScoreboardPanel/Content");
        if (contentParent == null) return;
        
        // Clear existing entries
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        // ALWAYS load from file only
        LoadFromFileOnly(contentParent.gameObject);
    }
    
    private void LoadFromFileOnly(GameObject contentParent)
    {
        // Load scores from file
        List<SimpleScoreEntry> fileScores = LoadScoresFromFile();
        
        if (fileScores.Count > 0)
        {
            // Show scores from file
            for (int i = 0; i < fileScores.Count && i < 10; i++)
            {
                SimpleScoreEntry entry = fileScores[i];
                CreateScoreEntry(contentParent, i + 1, entry.playerName, entry.score);
            }
        }
        else
        {
            // Show "No scores yet" message
            CreateNoScoresMessage(contentParent);
        }
    }
    
    private void CreateNoScoresMessage(GameObject contentParent)
    {
        GameObject messageObj = new GameObject("NoScoresMessage");
        messageObj.transform.SetParent(contentParent.transform, false);
        
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.sizeDelta = new Vector2(0, 100);
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "No scores yet - play some games!";
        messageText.fontSize = 24;
        messageText.color = Color.gray;
        messageText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement element = messageObj.AddComponent<LayoutElement>();
        element.preferredHeight = 100;
    }
    
    private void LoadRealGameData(GameObject contentParent)
    {
        Debug.Log("🔍 LoadRealGameData called - loading from file");
        
        // Load scores from the simple file first
        List<SimpleScoreEntry> fileScores = LoadScoresFromFile();
        Debug.Log($"📄 Loaded {fileScores.Count} scores from file");
        
        if (fileScores.Count > 0)
        {
            Debug.Log("📊 Creating score entries from file data");
            for (int i = 0; i < fileScores.Count && i < 10; i++)
            {
                SimpleScoreEntry entry = fileScores[i];
                Debug.Log($"   {i + 1}. {entry.playerName} - {entry.score} pts");
                CreateScoreEntry(contentParent, i + 1, entry.playerName, entry.score);
            }
            Debug.Log("✅ File scores displayed successfully");
            return;
        }
        
        Debug.Log("⚠️ No scores found in file, trying GameScoreManager fallback");
        
        // Fallback: Try to get real scores from GameScoreManager
        var gameScoreManager = FindObjectOfType<GameScoreManager>();
        if (gameScoreManager != null)
        {
            // Use reflection to access GetTopScores method if it exists
            var method = gameScoreManager.GetType().GetMethod("GetTopScores");
            if (method != null)
            {
                try
                {
                    var topScores = method.Invoke(gameScoreManager, new object[] { 10 }) as System.Collections.IList;
                    if (topScores != null && topScores.Count > 0)
                    {
                        for (int i = 0; i < topScores.Count; i++)
                        {
                            var scoreEntry = topScores[i];
                            var scoreType = scoreEntry.GetType();
                            
                            // Get player name
                            string playerName = "Unknown";
                            var nameField = scoreType.GetField("playerName");
                            var nameProperty = scoreType.GetProperty("playerName");
                            
                            if (nameField != null)
                            {
                                playerName = nameField.GetValue(scoreEntry)?.ToString() ?? "Unknown";
                            }
                            else if (nameProperty != null)
                            {
                                playerName = nameProperty.GetValue(scoreEntry, null)?.ToString() ?? "Unknown";
                            }
                            
                            // Get score
                            int score = 0;
                            var scoreField = scoreType.GetField("score");
                            var scoreProperty = scoreType.GetProperty("score");
                            
                            if (scoreField != null)
                            {
                                int.TryParse(scoreField.GetValue(scoreEntry)?.ToString(), out score);
                            }
                            else if (scoreProperty != null)
                            {
                                int.TryParse(scoreProperty.GetValue(scoreEntry, null)?.ToString(), out score);
                            }
                            
                            CreateScoreEntry(contentParent, i + 1, playerName, score);
                        }
                        
                        Debug.Log($"✅ Loaded {topScores.Count} real scores from GameScoreManager");
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ Error loading real scores: {e.Message}");
                }
            }
        }
        
        // Try PlayerPrefs as fallback
        LoadPlayerPrefsScores(contentParent);
    }
    
    [System.Serializable]
    public class ScoreEntry
    {
        public string name;
        public int score;
        
        public ScoreEntry(string playerName, int playerScore)
        {
            name = playerName;
            score = playerScore;
        }
    }
    
    private void LoadPlayerPrefsScores(GameObject contentParent)
    {
        // Load scores from PlayerPrefs
        List<ScoreEntry> scores = new List<ScoreEntry>();
        
        for (int i = 0; i < 20; i++) // Check first 20 slots
        {
            string playerName = PlayerPrefs.GetString($"Player_{i}_Name", "");
            int playerScore = PlayerPrefs.GetInt($"Player_{i}_Score", 0);
            
            if (!string.IsNullOrEmpty(playerName) && playerScore > 0)
            {
                scores.Add(new ScoreEntry(playerName, playerScore));
            }
        }
        
        if (scores.Count > 0)
        {
            // Sort by score descending
            scores.Sort((a, b) => b.score.CompareTo(a.score));
            
            // Display top scores
            for (int i = 0; i < Mathf.Min(scores.Count, 10); i++)
            {
                CreateScoreEntry(contentParent, i + 1, scores[i].name, scores[i].score);
            }
            
            Debug.Log($"✅ Loaded {scores.Count} scores from PlayerPrefs");
            return;
        }
        
        // If no real data found, show sample data with a note
        LoadSampleData(contentParent);
        Debug.Log("⚠️ No real game data found - showing sample data");
    }
    
    private void LoadSampleData(GameObject contentParent)
    {
        // Load sample data
        for (int i = 0; i < samplePlayerNames.Length && i < sampleScores.Length; i++)
        {
            CreateScoreEntry(contentParent, i + 1, samplePlayerNames[i], sampleScores[i]);
        }
    }
    
    private void ConnectToScoreboardButton()
    {
        // Find and connect to the scoreboard button
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        if (mainMenu != null)
        {
            // Use reflection to get the button
            var buttonField = typeof(MainMenuManager).GetField("scoreboardButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (buttonField != null)
            {
                Button scoreboardButton = buttonField.GetValue(mainMenu) as Button;
                if (scoreboardButton != null)
                {
                    // Clear existing listeners and add ours
                    scoreboardButton.onClick.RemoveAllListeners();
                    scoreboardButton.onClick.AddListener(() => {
                        Debug.Log("🎯 Scoreboard button clicked - showing working scoreboard");
                        ShowScoreboard();
                    });
                    
                    Debug.Log("✅ Connected to scoreboard button");
                }
            }
        }
    }
    
    [ContextMenu("📋 Show Scoreboard")]
    public void ShowScoreboard()
    {
        if (scoreboardCanvas == null)
        {
            CreateWorkingScoreboard();
        }
        
        if (scoreboardCanvas != null)
        {
            scoreboardCanvas.gameObject.SetActive(true);
            isVisible = true;
            
            // Hide main menu background - try multiple possible names
            HideMainMenuBackground();
            
            // Refresh scoreboard with real data
            RefreshScoreboardData();
            
            Debug.Log("✅ Working scoreboard shown!");
        }
    }
    
    private void HideMainMenuBackground()
    {
        // Try different possible main menu names
        string[] possibleMenuNames = {
            "MainMenuPanel", "Game Menu UI", "MainMenu", "MenuPanel", 
            "UI", "Canvas", "Main Menu", "MenuBackground"
        };
        
        foreach (string menuName in possibleMenuNames)
        {
            GameObject menu = GameObject.Find(menuName);
            if (menu != null && menu != scoreboardCanvas.gameObject)
            {
                // Don't fully disable, just hide visually
                Canvas menuCanvas = menu.GetComponent<Canvas>();
                if (menuCanvas != null)
                {
                    menuCanvas.enabled = false;
                    Debug.Log($"✅ Hidden main menu canvas: {menuName}");
                    break;
                }
                else
                {
                    // Try setting alpha to 0 for CanvasGroup
                    CanvasGroup canvasGroup = menu.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 0f;
                        Debug.Log($"✅ Hidden main menu via CanvasGroup: {menuName}");
                        break;
                    }
                }
            }
        }
    }
    
    [ContextMenu("❌ Hide Scoreboard")]
    public void HideScoreboard()
    {
        if (scoreboardCanvas != null)
        {
            scoreboardCanvas.gameObject.SetActive(false);
            isVisible = false;
            
            // Restore main menu background
            RestoreMainMenuBackground();
            
            Debug.Log("✅ Working scoreboard hidden");
        }
    }
    
    private void RestoreMainMenuBackground()
    {
        // Try different possible main menu names
        string[] possibleMenuNames = {
            "MainMenuPanel", "Game Menu UI", "MainMenu", "MenuPanel", 
            "UI", "Canvas", "Main Menu", "MenuBackground"
        };
        
        foreach (string menuName in possibleMenuNames)
        {
            GameObject menu = GameObject.Find(menuName);
            if (menu != null && menu != scoreboardCanvas.gameObject)
            {
                // Restore visibility
                Canvas menuCanvas = menu.GetComponent<Canvas>();
                if (menuCanvas != null)
                {
                    menuCanvas.enabled = true;
                    Debug.Log($"✅ Restored main menu canvas: {menuName}");
                    break;
                }
                else
                {
                    // Try restoring alpha for CanvasGroup
                    CanvasGroup canvasGroup = menu.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 1f;
                        Debug.Log($"✅ Restored main menu via CanvasGroup: {menuName}");
                        break;
                    }
                }
            }
        }
    }
    
    private void Update()
    {
        // Emergency close with multiple input methods
        if (isVisible)
        {
            bool shouldClose = false;
            
            // Keyboard ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                shouldClose = true;
                Debug.Log("🎮 Scoreboard closed via ESC key");
            }
            
            // Standard Cancel button
            if (Input.GetButtonDown("Cancel"))
            {
                shouldClose = true;
                Debug.Log("🎮 Scoreboard closed via Cancel button");
            }
            
            // Quest 3 B button (right controller) - multiple input methods
            if (Input.GetKeyDown(KeyCode.JoystickButton1) ||  // B button on most controllers
                Input.GetKeyDown(KeyCode.Joystick1Button1) || // Right controller B button
                Input.GetKeyDown(KeyCode.Joystick2Button1))   // Alternative mapping
            {
                shouldClose = true;
                Debug.Log("🎮 Scoreboard closed via Quest 3 B button (right controller)");
            }
            
            // Additional Quest 3 buttons
            if (Input.GetKeyDown(KeyCode.Joystick1Button6) || // Menu button right controller
                Input.GetKeyDown(KeyCode.Joystick1Button7))   // Alternative menu button
            {
                shouldClose = true;
                Debug.Log("🎮 Scoreboard closed via Quest 3 menu button");
            }
            
            // Check for XR Input if available (more reliable for Quest 3)
            if (CheckXRInput())
            {
                shouldClose = true;
                Debug.Log("🎮 Scoreboard closed via XR Input (Quest 3)");
            }
            
            if (shouldClose)
            {
                HideScoreboard();
            }
        }
    }
    
    private bool CheckXRInput()
    {
        // Try to check XR input using reflection to avoid compilation errors
        try
        {
            var ovrInputType = System.Type.GetType("OVRInput");
            if (ovrInputType != null)
            {
                // Check OVR B button (Quest specific)
                var getMethod = ovrInputType.GetMethod("GetDown", new[] { typeof(int) });
                if (getMethod != null)
                {
                    // OVRInput.Button.Two is typically the B button on right controller
                    var buttonTwoField = ovrInputType.GetField("Button")?.FieldType.GetField("Two");
                    if (buttonTwoField != null)
                    {
                        int buttonValue = (int)buttonTwoField.GetValue(null);
                        bool pressed = (bool)getMethod.Invoke(null, new object[] { buttonValue });
                        if (pressed) return true;
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // XR Input not available or failed - that's okay
        }
        
        return false;
    }
    
    public bool IsVisible => isVisible;
    
    // Public methods for external access
    public void ToggleScoreboard()
    {
        if (isVisible)
            HideScoreboard();
        else
            ShowScoreboard();
    }
    
    /// <summary>
    /// Call this when a player completes a game to add their score
    /// </summary>
    public static void AddPlayerScore(string playerName, int score, string mapName = "", string difficulty = "")
    {
        if (string.IsNullOrEmpty(playerName) || score <= 0) return;
        
        // Save to PlayerPrefs for persistence
        SaveScoreToPlayerPrefs(playerName, score, mapName, difficulty);
        
        // Try to also save to GameScoreManager if it exists
        SaveScoreToGameManager(playerName, score, mapName, difficulty);
        
        Debug.Log($"✅ Added score for {playerName}: {score} points");
    }
    
    private static void SaveScoreToPlayerPrefs(string playerName, int score, string mapName, string difficulty)
    {
        // Find an empty slot or lowest score to replace
        int slotToUse = -1;
        int lowestScore = int.MaxValue;
        int lowestScoreSlot = -1;
        
        for (int i = 0; i < 20; i++)
        {
            string existingName = PlayerPrefs.GetString($"Player_{i}_Name", "");
            int existingScore = PlayerPrefs.GetInt($"Player_{i}_Score", 0);
            
            // Found empty slot
            if (string.IsNullOrEmpty(existingName))
            {
                slotToUse = i;
                break;
            }
            
            // Track lowest score for potential replacement
            if (existingScore < lowestScore)
            {
                lowestScore = existingScore;
                lowestScoreSlot = i;
            }
        }
        
        // Use empty slot or replace lowest score if new score is higher
        if (slotToUse == -1 && score > lowestScore)
        {
            slotToUse = lowestScoreSlot;
        }
        
        if (slotToUse >= 0)
        {
            PlayerPrefs.SetString($"Player_{slotToUse}_Name", playerName);
            PlayerPrefs.SetInt($"Player_{slotToUse}_Score", score);
            PlayerPrefs.SetString($"Player_{slotToUse}_Map", mapName);
            PlayerPrefs.SetString($"Player_{slotToUse}_Difficulty", difficulty);
            PlayerPrefs.SetString($"Player_{slotToUse}_Date", System.DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            
            Debug.Log($"💾 Saved score to PlayerPrefs slot {slotToUse}");
        }
    }
    
    private static void SaveScoreToGameManager(string playerName, int score, string mapName, string difficulty)
    {
        var gameScoreManager = FindObjectOfType<GameScoreManager>();
        if (gameScoreManager != null)
        {
            try
            {
                // Try to call AddScore method if it exists
                var addScoreMethod = gameScoreManager.GetType().GetMethod("AddScore");
                if (addScoreMethod != null)
                {
                    // Try different parameter combinations
                    var parameters = addScoreMethod.GetParameters();
                    
                    if (parameters.Length == 2) // AddScore(string name, int score)
                    {
                        addScoreMethod.Invoke(gameScoreManager, new object[] { playerName, score });
                    }
                    else if (parameters.Length == 4) // AddScore(string name, int score, string map, string difficulty)
                    {
                        addScoreMethod.Invoke(gameScoreManager, new object[] { playerName, score, mapName, difficulty });
                    }
                    
                    Debug.Log("✅ Score saved to GameScoreManager");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Could not save to GameScoreManager: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Call this to force refresh the scoreboard with latest data
    /// </summary>
    public void RefreshScoreboard()
    {
        if (scoreboardCanvas != null && scoreboardCanvas.gameObject.activeInHierarchy)
        {
            RefreshScoreboardData();
        }
    }
    
    /// <summary>
    /// Clear all saved scores (for testing)
    /// </summary>
    [ContextMenu("🗑️ Clear All Scores")]
    public static void ClearAllScores()
    {
        for (int i = 0; i < 20; i++)
        {
            PlayerPrefs.DeleteKey($"Player_{i}_Name");
            PlayerPrefs.DeleteKey($"Player_{i}_Score");
            PlayerPrefs.DeleteKey($"Player_{i}_Map");
            PlayerPrefs.DeleteKey($"Player_{i}_Difficulty");
            PlayerPrefs.DeleteKey($"Player_{i}_Date");
        }
        PlayerPrefs.Save();
        Debug.Log("🗑️ All scores cleared");
    }
    
    [ContextMenu("📍 Position In Front Of Camera")]
    public void PositionInFrontOfCamera()
    {
        Camera cam = Camera.main ?? FindObjectOfType<Camera>();
        if (cam != null)
        {
            manualPosition = new Vector3(0f, 0f, 3f); // 3 units in front
            manualRotation = Vector3.zero;
            manualScale = 0.001f;
            useManualPositioning = true;
            
            // Apply immediately if scoreboard exists
            if (scoreboardCanvas != null)
            {
                PositionScoreboardLikeMenu(scoreboardCanvas.gameObject);
            }
            
            Debug.Log("📍 Positioned scoreboard in front of camera");
        }
    }
    
    [ContextMenu("📍 Position To The Right")]
    public void PositionToTheRight()
    {
        manualPosition = new Vector3(2f, 0f, 2f); // 2 units right, 2 units forward
        manualRotation = new Vector3(0f, -30f, 0f); // Angle towards player
        manualScale = 0.001f;
        useManualPositioning = true;
        
        // Apply immediately if scoreboard exists
        if (scoreboardCanvas != null)
        {
            PositionScoreboardLikeMenu(scoreboardCanvas.gameObject);
        }
        
        Debug.Log("📍 Positioned scoreboard to the right");
    }
    
    [ContextMenu("📍 Position To The Left")]
    public void PositionToTheLeft()
    {
        manualPosition = new Vector3(-2f, 0f, 2f); // 2 units left, 2 units forward
        manualRotation = new Vector3(0f, 30f, 0f); // Angle towards player
        manualScale = 0.001f;
        useManualPositioning = true;
        
        // Apply immediately if scoreboard exists
        if (scoreboardCanvas != null)
        {
            PositionScoreboardLikeMenu(scoreboardCanvas.gameObject);
        }
        
        Debug.Log("📍 Positioned scoreboard to the left");
    }
    
    [ContextMenu("📍 Use Menu Position (Default)")]
    public void UseMenuPosition()
    {
        useManualPositioning = false;
        
        // Apply immediately if scoreboard exists
        if (scoreboardCanvas != null)
        {
            PositionScoreboardLikeMenu(scoreboardCanvas.gameObject);
        }
        
        Debug.Log("📍 Scoreboard will now copy main menu position exactly");
    }
    
    [ContextMenu("🔍 Debug VR Raycasting")]
    public void DebugVRRaycasting()
    {
        Debug.Log("🔍 === VR RAYCASTING DIAGNOSTIC ===");
        
        // Compare main menu vs scoreboard settings
        GameObject mainMenu = FindMainMenu();
        if (mainMenu != null)
        {
            Debug.Log($"📋 MAIN MENU ({mainMenu.name}) - WORKING:");
            Canvas mainCanvas = mainMenu.GetComponent<Canvas>();
            GraphicRaycaster mainRaycaster = mainMenu.GetComponent<GraphicRaycaster>();
            
            if (mainCanvas != null)
            {
                Debug.Log($"   Canvas renderMode: {mainCanvas.renderMode}");
                Debug.Log($"   Canvas worldCamera: {mainCanvas.worldCamera?.name ?? "null"}");
                Debug.Log($"   Canvas sortingOrder: {mainCanvas.sortingOrder}");
            }
            
            if (mainRaycaster != null)
            {
                Debug.Log($"   GraphicRaycaster ignoreReversedGraphics: {mainRaycaster.ignoreReversedGraphics}");
                Debug.Log($"   GraphicRaycaster blockingObjects: {mainRaycaster.blockingObjects}");
                Debug.Log($"   GraphicRaycaster blockingMask: {mainRaycaster.blockingMask}");
            }
        }
        
        if (scoreboardCanvas != null)
        {
            Debug.Log($"📊 SCOREBOARD - TESTING:");
            GraphicRaycaster scoreRaycaster = scoreboardCanvas.GetComponent<GraphicRaycaster>();
            
            Debug.Log($"   Canvas renderMode: {scoreboardCanvas.renderMode}");
            Debug.Log($"   Canvas worldCamera: {scoreboardCanvas.worldCamera?.name ?? "null"}");
            Debug.Log($"   Canvas sortingOrder: {scoreboardCanvas.sortingOrder}");
            
            if (scoreRaycaster != null)
            {
                Debug.Log($"   GraphicRaycaster ignoreReversedGraphics: {scoreRaycaster.ignoreReversedGraphics}");
                Debug.Log($"   GraphicRaycaster blockingObjects: {scoreRaycaster.blockingObjects}");
                Debug.Log($"   GraphicRaycaster blockingMask: {scoreRaycaster.blockingMask}");
            }
        }
        
        Debug.Log("🔍 === END DIAGNOSTIC ===");
    }
    
    [ContextMenu("🔧 Fix VR Interaction")]
    public void FixVRInteraction()
    {
        if (scoreboardCanvas == null)
        {
            Debug.Log("ℹ️ Scoreboard canvas not created yet - creating it now...");
            CreateWorkingScoreboard();
        }
        
        if (scoreboardCanvas != null)
        {
            // Ensure GraphicRaycaster exists and is configured for VR
            GraphicRaycaster raycaster = scoreboardCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = scoreboardCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Added GraphicRaycaster to scoreboard");
            }
            
            // Configure for VR
            raycaster.ignoreReversedGraphics = false;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            
            Debug.Log("🔧 VR interaction fix applied to scoreboard canvas");
        }
        
        // Check if we have an XR Ray Interactor in the scene (this works regardless of canvas)
        CheckForXRRayInteractor();
        
        Debug.Log("✅ VR interaction diagnostic complete");
    }
    
    private void CheckForXRRayInteractor()
    {
        // Look for XR components that handle UI interaction
        var xrRayInteractors = FindObjectsOfType(System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRRayInteractor"));
        var xrUIInputModules = FindObjectsOfType(System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule"));
        
        if (xrRayInteractors != null && xrRayInteractors.Length > 0)
        {
            Debug.Log($"✅ Found {xrRayInteractors.Length} XR Ray Interactor(s) for UI interaction");
        }
        else
        {
            Debug.LogWarning("⚠️ No XR Ray Interactor found - you may need to add one for VR UI interaction");
        }
        
        if (xrUIInputModules != null && xrUIInputModules.Length > 0)
        {
            Debug.Log($"✅ Found {xrUIInputModules.Length} XR UI Input Module(s)");
        }
        else
        {
            Debug.LogWarning("⚠️ No XR UI Input Module found - you may need to add one to EventSystem");
        }
        
        // Check for EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log("✅ EventSystem found");
        }
        else
        {
            Debug.LogError("❌ No EventSystem found - UI interaction will not work!");
        }
    }
    
    private GameObject FindMainMenu()
    {
        // Try different possible main menu names in order of likelihood
        string[] possibleNames = { 
            "Game Menu UI",           // Your specific menu name
            "MainMenuPanel", 
            "MainMenu", 
            "MenuPanel", 
            "UI", 
            "Canvas",
            "Main Menu",
            "MenuCanvas",
            "GameUI"
        };
        
        foreach (string name in possibleNames)
        {
            GameObject menu = GameObject.Find(name);
            if (menu != null)
            {
                // Verify it has a Canvas component (likely a UI menu)
                Canvas canvas = menu.GetComponent<Canvas>();
                if (canvas != null)
                {
                    if (debugPositioning)
                    {
                        Debug.Log($"🎯 Found main menu: {name} (Canvas: {canvas.renderMode})");
                    }
                    return menu;
                }
            }
        }
        
        // If no named menu found, look for any Canvas with WorldSpace mode
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace && 
                canvas.gameObject != scoreboardCanvas?.gameObject) // Don't find ourselves
            {
                if (debugPositioning)
                {
                    Debug.Log($"🎯 Found WorldSpace Canvas: {canvas.name}");
                }
                return canvas.gameObject;
            }
        }
        
        Debug.LogWarning("⚠️ Could not find main menu - will use fallback positioning");
        return null;
    }
    
    [ContextMenu("🧪 Add Test Scores to File")]
    public void AddTestScoresToFile()
    {
        Debug.Log("🧪 Adding test scores to file...");
        
        SaveScore("Harry Potter", 2500, "ChamberOfSecrets", "Advanced");
        SaveScore("Hermione Granger", 2200, "DungeonsScene", "Advanced");
        SaveScore("Ron Weasley", 1800, "DungeonsScene", "Intermediate");
        SaveScore("Ginny Weasley", 1600, "ChamberOfSecrets", "Intermediate");
        SaveScore("Neville Longbottom", 1400, "DungeonsScene", "Beginner");
        
        Debug.Log("✅ Test scores added to file");
        
        // Refresh scoreboard if it's visible
        if (isVisible)
        {
            RefreshScoreboardData();
        }
    }
    
    [ContextMenu("🔄 Force Refresh from File")]
    public void ForceRefreshFromFile()
    {
        Debug.Log("🔄 Force refreshing scoreboard from file...");
        
        if (scoreboardCanvas != null)
        {
            RefreshScoreboardData();
        }
        else
        {
            Debug.LogWarning("❌ Scoreboard not created yet");
        }
    }
    
    [ContextMenu("🎮 Add My Current Score")]
    public void AddCurrentPlayerScore()
    {
        string playerName = PlayerPrefs.GetString("CurrentPlayerName", "Test Player");
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Generate a reasonable test score
        int baseScore = 1000;
        int difficultyBonus = difficulty * 200;
        int randomBonus = Random.Range(100, 500);
        int finalScore = baseScore + difficultyBonus + randomBonus;
        
        string difficultyName = GetDifficultyName(difficulty);
        
        WorkingScoreboard.AddPlayerScore(playerName, finalScore, mapName, difficultyName);
        
        Debug.Log($"🎮 Added score for {playerName}: {finalScore} points on {mapName} ({difficultyName})");
        
        // Refresh scoreboard if visible
        RefreshScoreboard();
    }
    
    private string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 0: return "Beginner";
            case 1: return "Intermediate"; 
            case 2: return "Advanced";
            default: return "Unknown";
        }
    }
    
    // Simple file loading methods
    [System.Serializable]
    public class SimpleScoreEntry
    {
        public string playerName;
        public int score;
        public string mapName;
        public string difficulty;
        public string timestamp;
    }
    
    [System.Serializable]
    public class SimpleScoreData
    {
        public List<SimpleScoreEntry> scores = new List<SimpleScoreEntry>();
    }
    
    private List<SimpleScoreEntry> LoadScoresFromFile()
    {
        string scoreFilePath = Path.Combine(Application.persistentDataPath, "hogwarts_scores.json");
        
        if (!File.Exists(scoreFilePath))
        {
            return new List<SimpleScoreEntry>();
        }
        
        try
        {
            string json = File.ReadAllText(scoreFilePath);
            if (string.IsNullOrEmpty(json))
            {
                return new List<SimpleScoreEntry>();
            }
            
            SimpleScoreData scoreData = JsonUtility.FromJson<SimpleScoreData>(json);
            return scoreData.scores ?? new List<SimpleScoreEntry>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scores: {e.Message}");
            return new List<SimpleScoreEntry>();
        }
    }
    
    // Static method to save scores from anywhere
    public static void SaveScore(string playerName, int score, string mapName = "", string difficulty = "")
    {
        string scoreFilePath = Path.Combine(Application.persistentDataPath, "hogwarts_scores.json");
        
        try
        {
            // Load existing scores
            SimpleScoreData scoreData = new SimpleScoreData();
            if (File.Exists(scoreFilePath))
            {
                string existingJson = File.ReadAllText(scoreFilePath);
                if (!string.IsNullOrEmpty(existingJson))
                {
                    scoreData = JsonUtility.FromJson<SimpleScoreData>(existingJson);
                }
            }
            
            // Add new score
            SimpleScoreEntry newEntry = new SimpleScoreEntry();
            newEntry.playerName = playerName;
            newEntry.score = score;
            newEntry.mapName = mapName;
            newEntry.difficulty = difficulty;
            newEntry.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            scoreData.scores.Add(newEntry);
            
            // Sort by score (highest first)
            scoreData.scores.Sort((a, b) => b.score.CompareTo(a.score));
            
            // Keep only top 20 scores
            if (scoreData.scores.Count > 20)
            {
                scoreData.scores.RemoveRange(20, scoreData.scores.Count - 20);
            }
            
            // Save back to file
            string json = JsonUtility.ToJson(scoreData, true);
            File.WriteAllText(scoreFilePath, json);
            
            Debug.Log($"💾 Score saved: {playerName} - {score} points");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving score: {e.Message}");
        }
    }
}
