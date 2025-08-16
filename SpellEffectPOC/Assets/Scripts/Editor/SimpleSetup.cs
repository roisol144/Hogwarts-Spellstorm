using UnityEngine;
using UnityEditor;

/// <summary>
/// Simple one-click setup for file-based scoreboard
/// </summary>
public class SimpleSetup : EditorWindow
{
    [MenuItem("Tools/📄 Simple File Scoreboard Setup")]
    public static void ShowWindow()
    {
        GetWindow<SimpleSetup>("File Scoreboard");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("📄 File-Based Scoreboard", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "SIMPLE SETUP:\n" +
            "• Your existing scoreboard will read from JSON file\n" +
            "• Game scenes automatically save scores to file\n" +
            "• File: persistentDataPath/hogwarts_scores.json",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (sceneName.ToLower().Contains("menu"))
        {
            EditorGUILayout.LabelField("🏠 MAIN MENU - Ready to show scores from file!", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🧪 Add Test Scores to File", GUILayout.Height(40)))
            {
                AddTestScores();
            }
        }
        else
        {
            EditorGUILayout.LabelField("🎮 GAME SCENE SETUP:", EditorStyles.boldLabel);
            
            SimplestScoreSaver saver = FindObjectOfType<SimplestScoreSaver>();
            if (saver != null)
            {
                EditorGUILayout.LabelField("✅ SimplestScoreSaver found!");
                
                if (GUILayout.Button("🏆 Test Victory Score", GUILayout.Height(30)))
                {
                    saver.TestVictory();
                }
                
                if (GUILayout.Button("💀 Test Death Score", GUILayout.Height(30)))
                {
                    saver.TestDeath();
                }
            }
            else
            {
                if (GUILayout.Button("🎮 ADD SCORE SAVER", GUILayout.Height(50)))
                {
                    AddScoreSaver();
                }
            }
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "HOW IT WORKS:\n" +
            "1. Add SimplestScoreSaver to game scenes\n" +
            "2. When you win/die, score saves to file\n" +
            "3. Your existing scoreboard reads from file\n" +
            "4. Scores appear automatically!",
            MessageType.None
        );
    }
    
    private void AddScoreSaver()
    {
        GameObject saverObj = new GameObject("SimplestScoreSaver");
        saverObj.AddComponent<SimplestScoreSaver>();
        
        Selection.activeGameObject = saverObj;
        EditorGUIUtility.PingObject(saverObj);
        
        MarkSceneDirty();
        
        EditorUtility.DisplayDialog("Score Saver Added!", 
            "✅ SimplestScoreSaver added!\n\n" +
            "Now when you:\n" +
            "• Win games → Victory scores saved to file\n" +
            "• Die in games → Death scores saved to file\n" +
            "• Return to main menu → Scores appear in scoreboard!\n\n" +
            "Right-click to test:\n" +
            "• 🏆 Test Victory\n" +
            "• 💀 Test Death",
            "Perfect!");
    }
    
    private void AddTestScores()
    {
        // Add some test scores directly to file
        WorkingScoreboard.SaveScore("Harry Potter", 2500, "ChamberOfSecrets", "Advanced");
        WorkingScoreboard.SaveScore("Hermione Granger", 2200, "DungeonsScene", "Advanced");
        WorkingScoreboard.SaveScore("Ron Weasley", 1800, "DungeonsScene", "Intermediate");
        WorkingScoreboard.SaveScore("Ginny Weasley", 1600, "ChamberOfSecrets", "Intermediate");
        WorkingScoreboard.SaveScore("Neville Longbottom", 1400, "DungeonsScene", "Beginner");
        
        EditorUtility.DisplayDialog("Test Scores Added!", 
            "✅ Added 5 test scores to file!\n\n" +
            "Now open your scoreboard in the main menu to see them.\n\n" +
            "File location: persistentDataPath/hogwarts_scores.json",
            "Great!");
    }
    
    private void MarkSceneDirty()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
