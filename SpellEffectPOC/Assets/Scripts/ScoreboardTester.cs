using UnityEngine;

/// <summary>
/// Quick tester to verify scoreboard integration works
/// </summary>
public class ScoreboardTester : MonoBehaviour
{
    [Header("🧪 Scoreboard Testing")]
    [SerializeField] private bool autoSetPlayerName = true;
    [SerializeField] private string testPlayerName = "Test Player";
    [SerializeField] private int testDifficulty = 1;

    private void Start()
    {
        if (autoSetPlayerName)
        {
            SetTestPlayerName();
        }
    }

    [ContextMenu("🎮 Set Test Player Name")]
    public void SetTestPlayerName()
    {
        PlayerPrefs.SetString("CurrentPlayerName", testPlayerName);
        PlayerPrefs.SetInt("SelectedDifficulty", testDifficulty);
        PlayerPrefs.Save();
        
        Debug.Log($"✅ Set test player: {testPlayerName} (Difficulty: {testDifficulty})");
    }

    [ContextMenu("🏆 Simulate Victory")]
    public void SimulateVictory()
    {
        // Find AutoScoreboardConnector and call public test method
        AutoScoreboardConnector connector = FindObjectOfType<AutoScoreboardConnector>();
        if (connector != null)
        {
            int testScore = Random.Range(1500, 3000);
            connector.TestVictoryScore(testScore);
            Debug.Log($"🏆 Simulated victory with score: {testScore}");
        }
        else
        {
            Debug.LogError("❌ AutoScoreboardConnector not found - add it first!");
        }
    }

    [ContextMenu("💀 Simulate Death")]
    public void SimulateDeath()
    {
        // Find AutoScoreboardConnector and call public test method
        AutoScoreboardConnector connector = FindObjectOfType<AutoScoreboardConnector>();
        if (connector != null)
        {
            connector.TestDeathScorePublic();
            Debug.Log($"💀 Simulated player death");
        }
        else
        {
            Debug.LogError("❌ AutoScoreboardConnector not found - add it first!");
        }
    }

    [ContextMenu("📊 Check Scoreboard")]
    public void CheckScoreboard()
    {
        WorkingScoreboard scoreboard = FindObjectOfType<WorkingScoreboard>();
        if (scoreboard == null)
        {
            // Create scoreboard if it doesn't exist
            GameObject scoreboardObj = new GameObject("WorkingScoreboardManager");
            scoreboard = scoreboardObj.AddComponent<WorkingScoreboard>();
            scoreboard.CreateWorkingScoreboard();
        }
        
        scoreboard.ShowScoreboard();
        Debug.Log("📊 Scoreboard opened");
    }

    [ContextMenu("🔍 Show Current Settings")]
    public void ShowCurrentSettings()
    {
        string playerName = PlayerPrefs.GetString("CurrentPlayerName", "Not Set");
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", -1);
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        Debug.Log("🔍 CURRENT SETTINGS:");
        Debug.Log($"   Player Name: {playerName}");
        Debug.Log($"   Difficulty: {difficulty}");
        Debug.Log($"   Scene: {sceneName}");
        
        // Check for AutoScoreboardConnector
        AutoScoreboardConnector connector = FindObjectOfType<AutoScoreboardConnector>();
        Debug.Log($"   AutoScoreboardConnector: {(connector != null ? "✅ Found" : "❌ Missing")}");
        
        // Check for game systems
        GameLevelManager levelManager = FindObjectOfType<GameLevelManager>();
        Debug.Log($"   GameLevelManager: {(levelManager != null ? "✅ Found" : "❌ Missing")}");
        
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        Debug.Log($"   PlayerHealth: {(playerHealth != null ? "✅ Found" : "❌ Missing")}");
    }

    [ContextMenu("🚀 Full Test Sequence")]
    public void FullTestSequence()
    {
        Debug.Log("🚀 STARTING FULL TEST SEQUENCE...");
        
        // 1. Set player name
        SetTestPlayerName();
        
        // 2. Show current settings
        ShowCurrentSettings();
        
        // 3. Test victory score
        Debug.Log("Testing victory score...");
        SimulateVictory();
        
        // 4. Wait a moment then test death score
        Invoke(nameof(TestDeathAfterDelay), 1f);
        
        // 5. Wait then show scoreboard
        Invoke(nameof(CheckScoreboard), 2f);
        
        Debug.Log("🚀 Full test sequence initiated! Check console and scoreboard in 3 seconds.");
    }
    
    private void TestDeathAfterDelay()
    {
        Debug.Log("Testing death score...");
        SimulateDeath();
    }
}
