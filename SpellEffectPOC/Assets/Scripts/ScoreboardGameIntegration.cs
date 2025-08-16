using UnityEngine;

/// <summary>
/// Simple integration to connect game events to the working scoreboard
/// Add this to your GameManager or similar object to automatically save scores
/// </summary>
public class ScoreboardGameIntegration : MonoBehaviour
{
    [Header("🎮 Game Integration Settings")]
    [SerializeField] private bool autoSaveScores = true;
    [SerializeField] private bool debugMode = true;
    
    [Header("📊 Score Calculation")]
    [SerializeField] private int baseWinScore = 1000;
    [SerializeField] private int difficultyMultiplier = 100;
    [SerializeField] private int timeBonus = 50; // Points per second remaining
    
    private string currentPlayerName;
    private string currentMap;
    private int currentDifficulty;
    private float gameStartTime;
    
    private void Start()
    {
        // Get player name from PlayerPrefs (set during name entry)
        currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        
        // Get current difficulty (0=Beginner, 1=Intermediate, 2=Advanced)
        currentDifficulty = PlayerPrefs.GetInt("GameDifficulty", 0);
        
        // Get current map name
        currentMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Record game start time
        gameStartTime = Time.time;
        
        if (debugMode)
        {
            Debug.Log($"🎮 Game started - Player: {currentPlayerName}, Map: {currentMap}, Difficulty: {GetDifficultyName(currentDifficulty)}");
        }
    }
    
    /// <summary>
    /// Call this when the player wins the game
    /// </summary>
    [ContextMenu("🏆 Test Game Won")]
    public void OnGameWon()
    {
        if (!autoSaveScores) return;
        
        int finalScore = CalculateFinalScore(true);
        string difficultyName = GetDifficultyName(currentDifficulty);
        
        // Add score to scoreboard
        WorkingScoreboard.AddPlayerScore(currentPlayerName, finalScore, currentMap, difficultyName);
        
        if (debugMode)
        {
            Debug.Log($"🏆 GAME WON! {currentPlayerName} scored {finalScore} points on {currentMap} ({difficultyName})");
        }
        
        // Optionally show scoreboard after game completion
        ShowScoreboardAfterDelay(2f);
    }
    
    /// <summary>
    /// Call this when the player loses the game
    /// </summary>
    [ContextMenu("💀 Test Game Lost")]
    public void OnGameLost()
    {
        if (!autoSaveScores) return;
        
        int finalScore = CalculateFinalScore(false);
        string difficultyName = GetDifficultyName(currentDifficulty);
        
        // Add score to scoreboard (lower score for loss)
        WorkingScoreboard.AddPlayerScore(currentPlayerName, finalScore, currentMap, difficultyName);
        
        if (debugMode)
        {
            Debug.Log($"💀 Game Lost. {currentPlayerName} scored {finalScore} points on {currentMap} ({difficultyName})");
        }
        
        // Optionally show scoreboard after game completion
        ShowScoreboardAfterDelay(3f);
    }
    
    /// <summary>
    /// Manual method to add a custom score
    /// </summary>
    public void AddCustomScore(string playerName, int score, bool won = true)
    {
        string difficultyName = GetDifficultyName(currentDifficulty);
        WorkingScoreboard.AddPlayerScore(playerName, score, currentMap, difficultyName);
        
        if (debugMode)
        {
            Debug.Log($"✅ Custom score added: {playerName} - {score} points");
        }
    }
    
    private int CalculateFinalScore(bool won)
    {
        int score = 0;
        
        if (won)
        {
            // Base score for winning
            score += baseWinScore;
            
            // Difficulty bonus
            score += currentDifficulty * difficultyMultiplier;
            
            // Time bonus (faster completion = higher score)
            float gameTime = Time.time - gameStartTime;
            float timeRemaining = Mathf.Max(0, 300f - gameTime); // Assume 5 minute target time
            score += Mathf.RoundToInt(timeRemaining * timeBonus);
        }
        else
        {
            // Consolation score for trying
            score = 100 + (currentDifficulty * 50);
            
            // Small time bonus even for losing
            float gameTime = Time.time - gameStartTime;
            if (gameTime > 60f) // Played for at least 1 minute
            {
                score += 50;
            }
        }
        
        return Mathf.Max(score, 0); // Ensure non-negative
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
    
    private void ShowScoreboardAfterDelay(float delay)
    {
        StartCoroutine(ShowScoreboardCoroutine(delay));
    }
    
    private System.Collections.IEnumerator ShowScoreboardCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Find and show the working scoreboard
        WorkingScoreboard scoreboard = FindObjectOfType<WorkingScoreboard>();
        if (scoreboard != null)
        {
            scoreboard.ShowScoreboard();
        }
        else
        {
            // Create one if it doesn't exist
            GameObject scoreboardObj = new GameObject("WorkingScoreboardManager");
            scoreboard = scoreboardObj.AddComponent<WorkingScoreboard>();
            scoreboard.CreateWorkingScoreboard();
            scoreboard.ShowScoreboard();
        }
    }
    
    #region Testing Methods
    [ContextMenu("🧪 Test Add Sample Scores")]
    public void AddSampleTestScores()
    {
        // Add some test scores for demonstration
        WorkingScoreboard.AddPlayerScore("Test Player 1", 2500, "DungeonsScene", "Advanced");
        WorkingScoreboard.AddPlayerScore("Test Player 2", 1800, "ChamberScene", "Intermediate");
        WorkingScoreboard.AddPlayerScore("Test Player 3", 1200, "DungeonsScene", "Beginner");
        
        Debug.Log("🧪 Added sample test scores");
    }
    
    [ContextMenu("🗑️ Clear Test Scores")]
    public void ClearTestScores()
    {
        WorkingScoreboard.ClearAllScores();
        Debug.Log("🗑️ Cleared all test scores");
    }
    
    [ContextMenu("📋 Show Scoreboard Now")]
    public void ShowScoreboardNow()
    {
        ShowScoreboardAfterDelay(0f);
    }
    #endregion
    
    #region Public Integration Methods
    /// <summary>
    /// Call this to set the current player name (usually from name input)
    /// </summary>
    public void SetPlayerName(string playerName)
    {
        currentPlayerName = playerName;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        
        if (debugMode)
        {
            Debug.Log($"👤 Player name set to: {playerName}");
        }
    }
    
    /// <summary>
    /// Call this to set the current difficulty
    /// </summary>
    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
        PlayerPrefs.SetInt("GameDifficulty", difficulty);
        PlayerPrefs.Save();
        
        if (debugMode)
        {
            Debug.Log($"⚡ Difficulty set to: {GetDifficultyName(difficulty)}");
        }
    }
    
    /// <summary>
    /// Get current player's name
    /// </summary>
    public string GetCurrentPlayerName()
    {
        return currentPlayerName;
    }
    
    /// <summary>
    /// Get current difficulty level
    /// </summary>
    public int GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
    #endregion
}
