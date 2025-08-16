using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tool to easily connect scoreboard to game scenes
/// </summary>
public class ScoreboardConnectionSetup : EditorWindow
{
    [MenuItem("Tools/🔗 Connect Game to Scoreboard")]
    public static void ShowWindow()
    {
        GetWindow<ScoreboardConnectionSetup>("Scoreboard Connection");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("🔗 Connect Game to Scoreboard", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "PROBLEM: Your name doesn't appear in scoreboard?\n" +
            "SOLUTION: Add AutoScoreboardConnector to your game scenes!\n\n" +
            "This will automatically save your score when you win or lose.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Check current scene
        string currentScene = SceneManager.GetActiveScene().name;
        EditorGUILayout.LabelField($"Current Scene: {currentScene}", EditorStyles.boldLabel);
        
        // Check for existing connector
        AutoScoreboardConnector existing = FindObjectOfType<AutoScoreboardConnector>();
        
        if (existing != null)
        {
            EditorGUILayout.LabelField("✅ AutoScoreboardConnector found!", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🔍 Show Current Player Info", GUILayout.Height(30)))
            {
                existing.ShowCurrentPlayerInfo();
            }
            
            if (GUILayout.Button("🧪 Test Save Score", GUILayout.Height(30)))
            {
                existing.TestSaveScore();
            }
            
            EditorGUILayout.Space();
            
                    if (GUILayout.Button("🔄 Reconnect to Game Systems", GUILayout.Height(25)))
        {
            existing.ConnectToGameSystems();
        }
        
        if (GUILayout.Button("🧪 Add Scoreboard Tester", GUILayout.Height(25)))
        {
            AddScoreboardTester();
        }
        }
        else
        {
            EditorGUILayout.LabelField("❌ AutoScoreboardConnector not found", EditorStyles.helpBox);
            
            if (GUILayout.Button("🔗 ADD SCOREBOARD CONNECTION", GUILayout.Height(50)))
            {
                AddScoreboardConnector();
            }
        }
        
        EditorGUILayout.Space();
        
        // Check for game systems
        EditorGUILayout.LabelField("🔍 Game Systems Status:", EditorStyles.boldLabel);
        
        GameLevelManager levelManager = FindObjectOfType<GameLevelManager>();
        EditorGUILayout.LabelField($"GameLevelManager: {(levelManager != null ? "✅ Found" : "❌ Missing")}");
        
        GameScoreManager scoreManager = FindObjectOfType<GameScoreManager>();
        EditorGUILayout.LabelField($"GameScoreManager: {(scoreManager != null ? "✅ Found" : "❌ Missing")}");
        
        // Check PlayerPrefs
        string playerName = PlayerPrefs.GetString("CurrentPlayerName", "Not Set");
        EditorGUILayout.LabelField($"Current Player: {playerName}");
        
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", -1);
        EditorGUILayout.LabelField($"Selected Difficulty: {difficulty}");
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "HOW IT WORKS:\n" +
            "1. AutoScoreboardConnector listens for game victory events\n" +
            "2. When you win, it saves your name and score to scoreboard\n" +
            "3. When you return to main menu, your score appears!\n\n" +
            "ADD TO GAME SCENES:\n" +
            "• DungeonsScene\n" +
            "• ChamberOfSecretsScene\n" +
            "• Any other playable scenes\n\n" +
            "✅ TRACKS BOTH WINS AND LOSSES:\n" +
            "• Victory scores: Full points for winning\n" +
            "• Death scores: Participation points for trying",
            MessageType.None
        );
    }
    
    private void AddScoreboardConnector()
    {
        // Create the connector
        GameObject connectorObj = new GameObject("AutoScoreboardConnector");
        AutoScoreboardConnector connector = connectorObj.AddComponent<AutoScoreboardConnector>();
        
        // Select it
        Selection.activeGameObject = connectorObj;
        EditorGUIUtility.PingObject(connectorObj);
        
        // Connect it immediately
        connector.ConnectToGameSystems();
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("Scoreboard Connected!", 
            "✅ AutoScoreboardConnector added to scene!\n\n" +
            "🎮 Now when you:\n" +
            "1. Enter your name in main menu\n" +
            "2. Play and complete the game\n" +
            "3. Return to main menu\n" +
            "4. Check scoreboard → Your score will be there!\n\n" +
            "🔧 Add this to ALL your game scenes for complete coverage.",
            "Great!");
    }
    
    private void AddScoreboardTester()
    {
        // Check if tester already exists
        ScoreboardTester existing = FindObjectOfType<ScoreboardTester>();
        if (existing != null)
        {
            EditorGUIUtility.PingObject(existing.gameObject);
            EditorUtility.DisplayDialog("Tester Found", 
                "ScoreboardTester already exists in scene!\n\n" +
                "Right-click the component to access test methods:\n" +
                "• 🏆 Simulate Victory\n" +
                "• 💀 Simulate Death\n" +
                "• 📊 Check Scoreboard\n" +
                "• 🚀 Full Test Sequence",
                "OK");
            return;
        }
        
        // Create the tester
        GameObject testerObj = new GameObject("ScoreboardTester");
        ScoreboardTester tester = testerObj.AddComponent<ScoreboardTester>();
        
        // Select it
        Selection.activeGameObject = testerObj;
        EditorGUIUtility.PingObject(testerObj);
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("Tester Added!", 
            "🧪 ScoreboardTester added to scene!\n\n" +
            "RIGHT-CLICK METHODS:\n" +
            "• 🏆 Simulate Victory - Test win scores\n" +
            "• 💀 Simulate Death - Test death scores\n" +
            "• 📊 Check Scoreboard - Open scoreboard\n" +
            "• 🚀 Full Test Sequence - Test everything\n\n" +
            "Perfect for testing without playing full games!",
            "Awesome!");
    }
}
