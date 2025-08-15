using UnityEngine;
using UnityEngine.InputSystem;

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
    
    [Header("VR Controller Input")]
    [Tooltip("Enable left controller X button for adding score")] 
    [SerializeField] private bool enableVRScoreInput = true;
    
    [Header("Input Actions")]
    [SerializeField] private InputAction leftXButtonAction;
    
    // Private variables
    private bool wasLeftXButtonPressed = false;
    
    void Start()
    {
        if (showDebugInfo)
        {
            LogCurrentLevelInfo();
        }
        
        // Setup VR input action if enabled
        if (enableVRScoreInput)
        {
            SetupVRInput();
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
        
        // Handle VR controller input
        if (enableVRScoreInput)
        {
            HandleVRInput();
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
    
    private void SetupVRInput()
    {
        // Setup input action for left controller X button
        if (leftXButtonAction.bindings.Count == 0)
        {
            leftXButtonAction = new InputAction("LeftXButton", InputActionType.Button);
            leftXButtonAction.AddBinding("<XRController>{LeftHand}/primaryButton");
            leftXButtonAction.AddBinding("<OculusTouchController>{LeftHand}/primaryButton");
        }
        
        Debug.Log($"[LevelSystemTester] VR input setup complete - Press X on left controller to gain {addScoreAmount} points");
    }
    
    void OnEnable()
    {
        if (enableVRScoreInput && leftXButtonAction != null)
        {
            leftXButtonAction.Enable();
        }
    }
    
    void OnDisable()
    {
        if (leftXButtonAction != null)
        {
            leftXButtonAction.Disable();
        }
    }
    
    private void HandleVRInput()
    {
        bool isLeftXButtonPressed = leftXButtonAction.ReadValue<float>() > 0.5f;
        
        // Check for button press (not hold)
        if (isLeftXButtonPressed && !wasLeftXButtonPressed)
        {
            AddTestScore();
        }
        
        wasLeftXButtonPressed = isLeftXButtonPressed;
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
        if (enableVRScoreInput)
        {
            GUILayout.Label($"Left Controller X - Add {addScoreAmount} Score (VR)");
        }
        GUILayout.Label($"{keyTestVictory} - Test Victory");
        GUILayout.Label($"{keyLogLevelInfo} - Log Level Info");
        
        GUILayout.EndArea();
    }
}