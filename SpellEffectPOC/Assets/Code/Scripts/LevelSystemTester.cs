using UnityEngine;

/// <summary>
/// Test script to verify the level system functionality
/// </summary>
public class LevelSystemTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Hotkeys (Configurable)")]
    [Tooltip("Key to log current level info to the console")] 
    [SerializeField] private KeyCode keyLogLevelInfo = KeyCode.L;
    
    [Tooltip("Key to switch to Level 1 (index 0)")] 
    [SerializeField] private KeyCode keySwitchLevel1 = KeyCode.Alpha1;
    [Tooltip("Key to switch to Level 2 (index 1)")] 
    [SerializeField] private KeyCode keySwitchLevel2 = KeyCode.Alpha2;
    [Tooltip("Key to switch to Level 3 (index 2)")] 
    [SerializeField] private KeyCode keySwitchLevel3 = KeyCode.Alpha3;
    
    [Tooltip("Key to trigger the victory test")] 
    [SerializeField] private KeyCode keyTestVictory = KeyCode.V;
    
    [Tooltip("Key to add score for testing")] 
    [SerializeField] private KeyCode keyAddScore = KeyCode.S;
    
    [Tooltip("Amount of score to add when the add-score hotkey is pressed")] 
    [SerializeField] private int addScoreAmount = 100;
    
    void Start()
    {
        if (showDebugInfo)
        {
            LogCurrentLevelInfo();
        }
    }
    
    void Update()
    {
        if (showDebugInfo && Input.GetKeyDown(keyLogLevelInfo))
        {
            LogCurrentLevelInfo();
        }
        
        // Test level switching with number keys
        if (Input.GetKeyDown(keySwitchLevel1))
        {
            SwitchToLevel(0, "Beginner");
        }
        else if (Input.GetKeyDown(keySwitchLevel2))
        {
            SwitchToLevel(1, "Intermediate");
        }
        else if (Input.GetKeyDown(keySwitchLevel3))
        {
            SwitchToLevel(2, "Advanced");
        }
        
        // Test victory condition
        if (Input.GetKeyDown(keyTestVictory))
        {
            TestVictory();
        }
        
        // Add score for testing
        if (Input.GetKeyDown(keyAddScore))
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
            ScoreManager.Instance.AddScore(addScoreAmount);
            Debug.Log($"[LevelSystemTester] Added {addScoreAmount} points!");
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
        GUILayout.Label($"{keySwitchLevel1}/{keySwitchLevel2}/{keySwitchLevel3} - Switch Levels");
        GUILayout.Label($"{keyAddScore} - Add {addScoreAmount} Score");
        GUILayout.Label($"{keyTestVictory} - Test Victory");
        GUILayout.Label($"{keyLogLevelInfo} - Log Level Info");
        
        GUILayout.EndArea();
    }
}