using UnityEngine;

public class ScoreManagerSetup : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int pointsPerKill = 10;
    [SerializeField] private bool setupOnStart = true;
    
    [Header("Debug/Testing")]
    [SerializeField] private bool showDebugButtons = true;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupScoreManager();
        }
    }
    
    [ContextMenu("Setup Score Manager")]
    public void SetupScoreManager()
    {
        Debug.Log("[ScoreManagerSetup] Setting up score manager...");
        
        // Find or create the score manager
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        GameObject scoreManagerObject;
        
        if (scoreManager == null)
        {
            // Create new GameObject for ScoreManager
            scoreManagerObject = new GameObject("ScoreManager");
            scoreManager = scoreManagerObject.AddComponent<ScoreManager>();
            Debug.Log($"[ScoreManagerSetup] Created new ScoreManager on GameObject: {scoreManagerObject.name}");
        }
        else
        {
            scoreManagerObject = scoreManager.gameObject;
            Debug.Log($"[ScoreManagerSetup] Found existing ScoreManager on GameObject: {scoreManagerObject.name}");
        }
        
        // Configure the score manager
        scoreManager.SetPointsPerKill(pointsPerKill);
        
        Debug.Log($"[ScoreManagerSetup] Score system setup complete!");
        Debug.Log($"[ScoreManagerSetup] Points per kill: {pointsPerKill}");
        Debug.Log($"[ScoreManagerSetup] Score display will appear in the top right corner of the player's view");
    }
    
    [ContextMenu("Test Add Kill Score")]
    public void TestAddKillScore()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScoreManagerSetup] Test can only be run in Play mode");
            return;
        }
        
        ScoreManager.NotifyKill();
        Debug.Log("[ScoreManagerSetup] Test kill score added");
    }
    
    [ContextMenu("Test Add Custom Score")]
    public void TestAddCustomScore()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScoreManagerSetup] Test can only be run in Play mode");
            return;
        }
        
        ScoreManager.NotifyScore(50);
        Debug.Log("[ScoreManagerSetup] Test custom score (50 points) added");
    }
    
    [ContextMenu("Reset Score")]
    public void TestResetScore()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScoreManagerSetup] Test can only be run in Play mode");
            return;
        }
        
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            Debug.Log("[ScoreManagerSetup] Score reset to 0");
        }
        else
        {
            Debug.LogWarning("[ScoreManagerSetup] No ScoreManager found to reset");
        }
    }
} 