using UnityEngine;

/// <summary>
/// Test script to verify the level system functionality
/// </summary>
public class LevelSystemTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (showDebugInfo)
        {
            LogCurrentLevelInfo();
        }
    }
    
    void Update()
    {
        if (showDebugInfo && Input.GetKeyDown(KeyCode.L))
        {
            LogCurrentLevelInfo();
        }
        
        // Test level switching with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToLevel(0, "Beginner");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToLevel(1, "Intermediate");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchToLevel(2, "Advanced");
        }
        
        // Test victory condition
        if (Input.GetKeyDown(KeyCode.V))
        {
            TestVictory();
        }
        
        // Add score for testing
        if (Input.GetKeyDown(KeyCode.S))
        {
            AddTestScore();
        }
    }
    
    private void LogCurrentLevelInfo()
    {
        if (GameLevelManager.Instance == null)
        {
            Debug.Log("[LevelSystemTester] GameLevelManager not found!");
            return;
        }
        
        var level = GameLevelManager.Instance.CurrentLevel;
        Debug.Log($"[LevelSystemTester] Current Level: {level.levelName}");
        Debug.Log($"[LevelSystemTester] Win Score: {level.winScore}");
        Debug.Log($"[LevelSystemTester] Enemy Spawn Interval: {level.enemySpawnInterval}s");
        Debug.Log($"[LevelSystemTester] Collectible Time Limit: {level.collectibleTimeLimit}s ({level.collectibleTimeLimit / 60f:F1} minutes)");
        
        // Check actual values in the spawners
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            Debug.Log($"[LevelSystemTester] Actual Enemy Spawner Interval: {enemySpawner.spawnInterval}s");
        }
        
        if (ScoreManager.Instance != null)
        {
            Debug.Log($"[LevelSystemTester] Current Score: {ScoreManager.Instance.GetCurrentScore()}");
        }
    }
    
    private void SwitchToLevel(int levelIndex, string levelName)
    {
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.SetLevel(levelIndex);
            Debug.Log($"[LevelSystemTester] Switched to {levelName} level!");
            LogCurrentLevelInfo();
        }
    }
    
    private void TestVictory()
    {
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.TestVictory();
            Debug.Log("[LevelSystemTester] Triggered test victory!");
        }
    }
    
    private void AddTestScore()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
            Debug.Log("[LevelSystemTester] Added 100 points!");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Level System Tester", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
        
        if (GameLevelManager.Instance != null)
        {
            var level = GameLevelManager.Instance.CurrentLevel;
            GUILayout.Label($"Current Level: {level.levelName}");
            GUILayout.Label($"Win Score: {level.winScore}");
            GUILayout.Label($"Enemy Spawn: {level.enemySpawnInterval}s");
            GUILayout.Label($"Collectible Time: {level.collectibleTimeLimit}s");
            
            if (ScoreManager.Instance != null)
            {
                int currentScore = ScoreManager.Instance.GetCurrentScore();
                GUILayout.Label($"Current Score: {currentScore}");
                
                // Show progress towards victory
                float progress = (float)currentScore / level.winScore;
                GUILayout.Label($"Progress: {progress * 100f:F1}%");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("1/2/3 - Switch Levels");
        GUILayout.Label("S - Add 100 Score");
        GUILayout.Label("V - Test Victory");
        GUILayout.Label("L - Log Level Info");
        
        GUILayout.EndArea();
    }
}