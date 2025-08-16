using UnityEngine;

/// <summary>
/// SIMPLEST SCORE SAVER
/// Just saves scores to file when game ends - that's it!
/// </summary>
public class SimplestScoreSaver : MonoBehaviour
{
    [Header("🎮 Simplest Score Saver")]
    [SerializeField] private bool debugMode = true;
    
    private string playerName;
    private bool hasRecordedScore = false;
    
    private void Start()
    {
        // Get player name from main menu
        playerName = PlayerPrefs.GetString("CurrentPlayerName", "Player");
        
        // Try to connect to game events
        try
        {
            GameLevelManager.OnVictoryAchieved += OnVictory;
            
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += OnDeath;
            }
            
            if (debugMode)
            {
                Debug.Log($"🎮 SimplestScoreSaver ready for: {playerName}");
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogWarning($"Could not connect to game events: {e.Message}");
            }
        }
    }
    
    private void OnVictory(int gameScore)
    {
        if (hasRecordedScore) return;
        
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        int finalScore = gameScore + (difficulty * 200);
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string difficultyName = GetDifficultyName(difficulty);
        
        // Save to file using WorkingScoreboard static method
        WorkingScoreboard.SaveScore(playerName, finalScore, mapName, difficultyName);
        
        hasRecordedScore = true;
        
        if (debugMode)
        {
            Debug.Log($"🏆 Victory score saved: {playerName} - {finalScore} points");
        }
    }
    
    private void OnDeath()
    {
        if (hasRecordedScore) return;
        
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        int finalScore = 150 + (difficulty * 50);
        string mapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string difficultyName = GetDifficultyName(difficulty);
        
        // Save to file using WorkingScoreboard static method
        WorkingScoreboard.SaveScore(playerName, finalScore, mapName, difficultyName);
        
        hasRecordedScore = true;
        
        if (debugMode)
        {
            Debug.Log($"💀 Death score saved: {playerName} - {finalScore} points");
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
    
    [ContextMenu("🏆 Test Victory")]
    public void TestVictory()
    {
        OnVictory(Random.Range(1000, 3000));
    }
    
    [ContextMenu("💀 Test Death")]
    public void TestDeath()
    {
        OnDeath();
    }
    
    private void OnDestroy()
    {
        try
        {
            GameLevelManager.OnVictoryAchieved -= OnVictory;
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= OnDeath;
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
