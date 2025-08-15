using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles game completion and score recording
/// Add this to your game scene and call RecordScore when the player finishes
/// </summary>
public class GameCompletionHandler : MonoBehaviour
{
    [Header("Score Recording")]
    [SerializeField] private bool autoRecordOnGameEnd = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Record the player's score and optionally return to main menu
    /// </summary>
    /// <param name="finalScore">The player's final score</param>
    /// <param name="returnToMenu">Whether to automatically return to main menu</param>
    public void RecordScore(int finalScore, bool returnToMenu = true)
    {
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.EndGameSession(finalScore);
            Debug.Log($"GameCompletionHandler: Recorded score of {finalScore} points");
            
            if (returnToMenu)
            {
                // Small delay to ensure score is saved before scene change
                Invoke(nameof(ReturnToMainMenu), 1f);
            }
        }
        else
        {
            Debug.LogError("GameCompletionHandler: GameScoreManager not found! Cannot record score.");
        }
    }

    /// <summary>
    /// Record score and show a completion message
    /// </summary>
    /// <param name="finalScore">The player's final score</param>
    /// <param name="completionMessage">Message to show to player</param>
    public void RecordScoreWithMessage(int finalScore, string completionMessage)
    {
        Debug.Log($"Game Complete: {completionMessage} - Score: {finalScore}");
        RecordScore(finalScore, false);
        
        // You can extend this to show a proper UI completion screen
        ShowCompletionMessage(completionMessage, finalScore);
    }

    /// <summary>
    /// Simple completion message display (can be enhanced with proper UI)
    /// </summary>
    private void ShowCompletionMessage(string message, int score)
    {
        // For now, just log it - you can create a proper UI for this later
        Debug.Log($"=== GAME COMPLETE ===");
        Debug.Log($"Message: {message}");
        Debug.Log($"Final Score: {score}");
        Debug.Log($"Press any key to return to main menu...");
        
        // Simple input to return to menu
        StartCoroutine(WaitForInputToReturnToMenu());
    }

    private System.Collections.IEnumerator WaitForInputToReturnToMenu()
    {
        yield return new WaitForSeconds(1f); // Brief pause
        
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        
        ReturnToMainMenu();
    }

    /// <summary>
    /// Return to the main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("GameCompletionHandler: Returning to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Quick method for testing - call this from inspector or other scripts
    /// </summary>
    [ContextMenu("Test Record Score")]
    public void TestRecordScore()
    {
        int testScore = Random.Range(500, 2000);
        RecordScore(testScore, false);
        Debug.Log($"Test score recorded: {testScore}");
    }

    /// <summary>
    /// Example method showing how different game scenarios might record scores
    /// </summary>
    public void HandleGameEnd(string endReason, int baseScore, float timeBonus = 0f, int perfectBonus = 0)
    {
        int finalScore = baseScore + Mathf.RoundToInt(timeBonus) + perfectBonus;
        
        string completionMessage = endReason switch
        {
            "victory" => $"Congratulations! You completed the challenge!",
            "defeat" => $"Game Over. Better luck next time!",
            "timeout" => $"Time's up! Here's your score so far.",
            _ => $"Game ended: {endReason}"
        };

        RecordScoreWithMessage(finalScore, completionMessage);
    }

    #region Example Integration Methods

    /// <summary>
    /// Example: Call this when player completes dungeon level
    /// </summary>
    public void OnDungeonComplete(int enemiesDefeated, int spellsCast, float timeRemaining)
    {
        int baseScore = enemiesDefeated * 100;
        int spellBonus = spellsCast * 10;
        int timeBonus = Mathf.RoundToInt(timeRemaining * 5);
        
        int finalScore = baseScore + spellBonus + timeBonus;
        
        RecordScoreWithMessage(finalScore, $"Dungeon cleared! Enemies: {enemiesDefeated}, Spells: {spellsCast}");
    }

    /// <summary>
    /// Example: Call this when player completes chamber level
    /// </summary>
    public void OnChamberComplete(bool basiliskDefeated, int treasuresFound, float completionTime)
    {
        int baseScore = basiliskDefeated ? 1000 : 500;
        int treasureBonus = treasuresFound * 200;
        int timeBonus = completionTime < 300f ? 500 : 0; // Bonus for completing under 5 minutes
        
        int finalScore = baseScore + treasureBonus + timeBonus;
        
        string message = basiliskDefeated ? 
            $"Basilisk defeated! Treasures found: {treasuresFound}" : 
            $"Chamber explored! Treasures found: {treasuresFound}";
            
        RecordScoreWithMessage(finalScore, message);
    }

    #endregion
}
