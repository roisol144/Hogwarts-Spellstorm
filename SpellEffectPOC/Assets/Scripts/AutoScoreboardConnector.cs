using UnityEngine;

/// <summary>
/// Automatically connects the game completion events to the scoreboard system
/// Add this to your game scenes to automatically save scores when games end
/// </summary>
public class AutoScoreboardConnector : MonoBehaviour
{
    [Header("🔗 Auto Scoreboard Connection")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool autoConnect = true;
    
    [Header("📊 Score Settings")]
    [SerializeField] private int minScoreToSave = 10; // Don't save very low scores
    
    private GameLevelManager gameLevelManager;
    private GameScoreManager gameScoreManager;
    private PlayerHealth playerHealth;
    private string currentPlayerName;
    private bool isConnected = false;
    
    private void Start()
    {
        if (autoConnect)
        {
            ConnectToGameSystems();
        }
    }
    
    [ContextMenu("🔗 Connect To Game Systems")]
    public void ConnectToGameSystems()
    {
        // Find game systems
        gameLevelManager = FindObjectOfType<GameLevelManager>();
        gameScoreManager = FindObjectOfType<GameScoreManager>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        // Get current player name
        currentPlayerName = PlayerPrefs.GetString("CurrentPlayerName", "Unknown Player");
        
        // Connect to victory event (static event)
        GameLevelManager.OnVictoryAchieved += OnGameVictory;
        
        // Connect to death event
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += OnPlayerDeath;
            isConnected = true;
            
            if (debugMode)
            {
                Debug.Log($"✅ Connected to PlayerHealth death events for player: {currentPlayerName}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerHealth not found - death scores won't be saved to scoreboard");
        }
        
        // Check for GameLevelManager
        if (gameLevelManager != null)
        {
            if (debugMode)
            {
                Debug.Log($"✅ Found GameLevelManager - connected to victory events for player: {currentPlayerName}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameLevelManager not found - victory scores won't be saved to scoreboard");
        }
        
        // Connect to GameScoreManager if available
        if (gameScoreManager != null)
        {
            if (debugMode)
            {
                Debug.Log("✅ Found GameScoreManager - will use for score data");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameScoreManager not found - using basic score calculation");
        }
        
        if (debugMode)
        {
            Debug.Log($"🎮 AutoScoreboardConnector ready for player: {currentPlayerName}");
            Debug.Log($"   Victory events: {(gameLevelManager != null ? "✅" : "❌")}");
            Debug.Log($"   Death events: {(playerHealth != null ? "✅" : "❌")}");
        }
    }
    
    /// <summary>
    /// Called when player achieves victory
    /// </summary>
    private void OnGameVictory(int finalScore)
    {
        if (finalScore < minScoreToSave)
        {
            if (debugMode)
            {
                Debug.Log($"⚠️ Score {finalScore} too low to save (minimum: {minScoreToSave})");
            }
            return;
        }
        
        // Get current game info
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        string difficultyName = GetDifficultyName(difficulty);
        
        // Save to working scoreboard
        WorkingScoreboard.AddPlayerScore(currentPlayerName, finalScore, mapName, difficultyName);
        
        if (debugMode)
        {
            Debug.Log($"🏆 VICTORY SCORE SAVED!");
            Debug.Log($"   Player: {currentPlayerName}");
            Debug.Log($"   Score: {finalScore}");
            Debug.Log($"   Map: {mapName}");
            Debug.Log($"   Difficulty: {difficultyName}");
        }
        
        // Also save to GameScoreManager if available
        if (gameScoreManager != null)
        {
            gameScoreManager.EndGameSession(finalScore);
            if (debugMode)
            {
                Debug.Log("💾 Also saved to GameScoreManager");
            }
        }
    }
    
    /// <summary>
    /// Called when player dies
    /// </summary>
    private void OnPlayerDeath()
    {
        // Calculate a participation score based on current game progress
        int participationScore = CalculateDeathScore();
        
        if (participationScore < minScoreToSave)
        {
            if (debugMode)
            {
                Debug.Log($"⚠️ Death score {participationScore} too low to save (minimum: {minScoreToSave})");
            }
            return;
        }
        
        // Get current game info
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        string difficultyName = GetDifficultyName(difficulty);
        
        // Save to working scoreboard
        WorkingScoreboard.AddPlayerScore(currentPlayerName, participationScore, mapName, difficultyName);
        
        if (debugMode)
        {
            Debug.Log($"💀 DEATH SCORE SAVED!");
            Debug.Log($"   Player: {currentPlayerName}");
            Debug.Log($"   Score: {participationScore}");
            Debug.Log($"   Map: {mapName}");
            Debug.Log($"   Difficulty: {difficultyName}");
        }
        
        // Also save to GameScoreManager if available
        if (gameScoreManager != null)
        {
            gameScoreManager.EndGameSession(participationScore);
            if (debugMode)
            {
                Debug.Log("💾 Death score also saved to GameScoreManager");
            }
        }
    }
    
    /// <summary>
    /// Calculate score based on current game progress when player dies
    /// </summary>
    private int CalculateDeathScore()
    {
        int baseScore = 100; // Base participation score
        
        // Get difficulty bonus
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        int difficultyBonus = difficulty * 50;
        
        // Try to get current score from ScoreManager
        int currentGameScore = 0;
        try
        {
            // Try to get current score from ScoreManager (reflection safe)
            var scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                // Use reflection to safely get current score
                var scoreField = scoreManager.GetType().GetField("currentScore", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (scoreField != null)
                {
                    currentGameScore = (int)scoreField.GetValue(scoreManager);
                }
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.Log($"Could not get current score: {e.Message}");
            }
        }
        
        // Calculate final death score
        int finalScore = baseScore + difficultyBonus + (currentGameScore / 2); // Half of achieved score
        
        if (debugMode)
        {
            Debug.Log($"💀 Death score calculation:");
            Debug.Log($"   Base: {baseScore}");
            Debug.Log($"   Difficulty bonus: {difficultyBonus}");
            Debug.Log($"   Game progress: {currentGameScore} (using {currentGameScore / 2})");
            Debug.Log($"   Final death score: {finalScore}");
        }
        
        return finalScore;
    }
    
    /// <summary>
    /// Manual method to save current score (call this from other scripts)
    /// </summary>
    public void SaveCurrentScore(int score, bool isVictory = true)
    {
        if (string.IsNullOrEmpty(currentPlayerName))
        {
            currentPlayerName = PlayerPrefs.GetString("CurrentPlayerName", "Unknown Player");
        }
        
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        string difficultyName = GetDifficultyName(difficulty);
        
        WorkingScoreboard.AddPlayerScore(currentPlayerName, score, mapName, difficultyName);
        
        if (debugMode)
        {
            string resultType = isVictory ? "VICTORY" : "GAME END";
            Debug.Log($"🎯 {resultType} SCORE SAVED: {currentPlayerName} - {score} points on {mapName} ({difficultyName})");
        }
    }
    
    /// <summary>
    /// Call this when player loses/dies (optional - for participation scores)
    /// </summary>
    public void OnGameLoss(int participationScore = 0)
    {
        // Calculate a small participation score if none provided
        if (participationScore <= 0)
        {
            int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
            participationScore = 50 + (difficulty * 25); // 50-100 points for trying
        }
        
        if (participationScore >= minScoreToSave)
        {
            SaveCurrentScore(participationScore, false);
        }
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
    
    [ContextMenu("🧪 Test Save Score")]
    public void TestSaveScore()
    {
        int testScore = Random.Range(500, 2500);
        SaveCurrentScore(testScore, true);
        Debug.Log($"🧪 Test score saved: {testScore}");
    }
    
    [ContextMenu("💀 Test Death Score")]
    public void TestDeathScore()
    {
        OnPlayerDeath();
        Debug.Log($"💀 Test death score triggered");
    }
    
    /// <summary>
    /// Public method to test victory scores (for ScoreboardTester)
    /// </summary>
    public void TestVictoryScore(int score = -1)
    {
        if (score < 0)
        {
            score = Random.Range(1500, 3000);
        }
        OnGameVictory(score);
        Debug.Log($"🏆 Test victory score triggered: {score}");
    }
    
    /// <summary>
    /// Public method to test death scores (for ScoreboardTester)
    /// </summary>
    public void TestDeathScorePublic()
    {
        OnPlayerDeath();
        Debug.Log($"💀 Test death score triggered");
    }
    
    [ContextMenu("📋 Show Current Player Info")]
    public void ShowCurrentPlayerInfo()
    {
        string playerName = PlayerPrefs.GetString("CurrentPlayerName", "Not Set");
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", -1);
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Debug.Log("📋 CURRENT GAME INFO:");
        Debug.Log($"   Player: {playerName}");
        Debug.Log($"   Map: {mapName}");
        Debug.Log($"   Difficulty: {difficulty} ({GetDifficultyName(difficulty)})");
        Debug.Log($"   Connected: {isConnected}");
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        GameLevelManager.OnVictoryAchieved -= OnGameVictory;
        
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= OnPlayerDeath;
        }
    }
}
