using UnityEngine;
using UnityEditor;

/// <summary>
/// Simple setup for the working scoreboard
/// </summary>
public class WorkingScoreboardSetup : EditorWindow
{
    [MenuItem("Tools/✅ Create WORKING Scoreboard")]
    public static void ShowWindow()
    {
        GetWindow<WorkingScoreboardSetup>("Working Scoreboard");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("✅ WORKING Scoreboard Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "SIMPLE MENU REPLACEMENT:\n" +
            "• Automatically finds and copies your main menu position\n" +
            "• Appears exactly where your menu is (no behind-you issues)\n" +
            "• Static positioning (doesn't follow head)\n" +
            "• Manual positioning available if needed\n" +
            "• Clean menu replacement",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Check status
        WorkingScoreboard existing = FindObjectOfType<WorkingScoreboard>();
        
        if (existing == null)
        {
            EditorGUILayout.LabelField("❌ Working scoreboard not found", EditorStyles.helpBox);
            
            if (GUILayout.Button("🚀 CREATE WORKING SCOREBOARD", GUILayout.Height(50)))
            {
                CreateWorkingScoreboard();
            }
        }
        else
        {
            EditorGUILayout.LabelField("✅ Working scoreboard found!", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🧪 Test Show Scoreboard", GUILayout.Height(30)))
            {
                existing.ShowScoreboard();
            }
            
            if (GUILayout.Button("❌ Test Hide Scoreboard", GUILayout.Height(30)))
            {
                existing.HideScoreboard();
            }
            
            EditorGUILayout.Space();
            
                    if (GUILayout.Button("🔄 Recreate Scoreboard", GUILayout.Height(25)))
        {
            RecreateScoreboard();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🎮 Add Game Integration", GUILayout.Height(30)))
        {
            AddGameIntegration();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🔧 Check & Fix VR Interaction", GUILayout.Height(30)))
        {
            FixVRInteraction();
        }
        }
        
        EditorGUILayout.Space();
        
        // Status info
        EditorGUILayout.LabelField("📊 Scene Status:", EditorStyles.boldLabel);
        
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        EditorGUILayout.LabelField($"MainMenuManager: {(mainMenu != null ? "✅ Found" : "❌ Missing")}");
        
        Camera camera = Camera.main ?? FindObjectOfType<Camera>();
        EditorGUILayout.LabelField($"Camera: {(camera != null ? "✅ " + camera.name : "❌ Missing")}");
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "DEFAULT BEHAVIOR:\n" +
            "• Scoreboard appears exactly where your main menu is\n" +
            "• No position adjustments needed\n" +
            "• If you want custom positioning:\n" +
            "  1. Select WorkingScoreboardManager in scene\n" +
            "  2. Check 'Use Manual Positioning'\n" +
            "  3. Or right-click component → 'Use Menu Position'\n\n" +
            "Simple and automatic - just like your menu!",
            MessageType.None
        );
    }
    
    private void CreateWorkingScoreboard()
    {
        Debug.Log("🚀 Creating WORKING scoreboard...");
        
        // Create the working scoreboard object
        GameObject scoreboardObj = new GameObject("WorkingScoreboardManager");
        WorkingScoreboard scoreboard = scoreboardObj.AddComponent<WorkingScoreboard>();
        
        // Create the scoreboard
        scoreboard.CreateWorkingScoreboard();
        
        // Select it
        Selection.activeGameObject = scoreboardObj;
        EditorGUIUtility.PingObject(scoreboardObj);
        
        EditorUtility.DisplayDialog("Success!", 
            "✅ WORKING Scoreboard Created!\n\n" +
            "🎮 Test it now:\n" +
            "1. Run the scene\n" +
            "2. Click the Scoreboard button\n" +
            "3. Enjoy your working VR scoreboard!\n\n" +
            "Uses Screen Space Camera - guaranteed to work in VR!",
            "Awesome!");
    }
    
    private void RecreateScoreboard()
    {
        // Remove existing
        WorkingScoreboard[] existing = FindObjectsOfType<WorkingScoreboard>();
        foreach (var scoreboard in existing)
        {
            DestroyImmediate(scoreboard.gameObject);
        }
        
        // Remove any working scoreboard canvases
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas.name.Contains("WorkingScoreboard"))
            {
                DestroyImmediate(canvas.gameObject);
            }
        }
        
        Debug.Log("🧹 Cleaned up old scoreboard");
        
        // Create new one
        CreateWorkingScoreboard();
    }
    
    private void AddGameIntegration()
    {
        // Check if integration already exists
        ScoreboardGameIntegration existing = FindObjectOfType<ScoreboardGameIntegration>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists", 
                "✅ ScoreboardGameIntegration already exists!\n\n" +
                "Found on: " + existing.gameObject.name,
                "OK");
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            return;
        }
        
        // Create game integration
        GameObject integrationObj = new GameObject("ScoreboardGameIntegration");
        ScoreboardGameIntegration integration = integrationObj.AddComponent<ScoreboardGameIntegration>();
        
        // Select it
        Selection.activeGameObject = integrationObj;
        EditorGUIUtility.PingObject(integrationObj);
        
        EditorUtility.DisplayDialog("Game Integration Added!", 
            "✅ ScoreboardGameIntegration created!\n\n" +
            "🎮 How to use:\n" +
            "1. Call integration.OnGameWon() when player wins\n" +
            "2. Call integration.OnGameLost() when player loses\n" +
            "3. Call integration.SetPlayerName(name) when player enters name\n" +
            "4. Call integration.SetDifficulty(level) when difficulty is selected\n\n" +
            "The integration will automatically save scores to the scoreboard!",
            "Awesome!");
    }
    
    private void FixVRInteraction()
    {
        WorkingScoreboard scoreboard = FindObjectOfType<WorkingScoreboard>();
        if (scoreboard != null)
        {
            // Call the VR interaction fix
            scoreboard.FixVRInteraction();
            
            EditorUtility.DisplayDialog("VR Interaction Check", 
                "✅ VR interaction diagnostic complete!\n\n" +
                "Check the Console for detailed information about:\n" +
                "• Scoreboard canvas and GraphicRaycaster\n" +
                "• XR Ray Interactor detection\n" +
                "• EventSystem presence\n\n" +
                "If issues are reported, your VR setup may be missing components.\n" +
                "The scoreboard will now work the same as your main menu!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Scoreboard Found", 
                "❌ No WorkingScoreboard found!\n\n" +
                "Create the scoreboard first, then run this fix.",
                "OK");
        }
    }
}
