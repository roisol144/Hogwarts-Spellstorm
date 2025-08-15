using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[System.Serializable]
public class GameScoreEntry
{
    public string playerName;
    public int score;
    public string mapName;
    public string difficulty;
    public string dateTime;
    public float playTime; // in seconds

    public GameScoreEntry(string name, int playerScore, string map, string diff, float time)
    {
        playerName = name;
        score = playerScore;
        mapName = map;
        difficulty = diff;
        dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        playTime = time;
    }
}

[System.Serializable]
public class GameScoreData
{
    public List<GameScoreEntry> scores = new List<GameScoreEntry>();
}

/// <summary>
/// Manages all score-related functionality including saving/loading scores to/from local file
/// </summary>
public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance { get; private set; }

    [Header("Score Configuration")]
    [SerializeField] private int maxScoreEntries = 50; // Maximum number of scores to keep
    [SerializeField] private string scoreFileName = "hogwarts_scores.json";

    private GameScoreData scoreData;
    private string scoreFilePath;

    // Current game session data
    private string currentPlayerName = "";
    private string currentMapName = "";
    private string currentDifficulty = "";
    private float gameStartTime = 0f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeScoreSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeScoreSystem()
    {
        scoreFilePath = Path.Combine(Application.persistentDataPath, scoreFileName);
        LoadScores();
        Debug.Log($"ScoreManager initialized. Score file: {scoreFilePath}");
    }

    #region Player Session Management

    /// <summary>
    /// Set the current player name from the menu system
    /// </summary>
    public void SetCurrentPlayer(string playerName)
    {
        currentPlayerName = playerName.Trim();
        Debug.Log($"ScoreManager: Set current player to '{currentPlayerName}'");
    }

    /// <summary>
    /// Set the current game session details
    /// </summary>
    public void StartGameSession(string playerName, string mapName, string difficulty)
    {
        currentPlayerName = playerName.Trim();
        currentMapName = mapName;
        currentDifficulty = GetDifficultyName(difficulty);
        gameStartTime = Time.time;
        
        Debug.Log($"ScoreManager: Started game session - Player: {currentPlayerName}, Map: {currentMapName}, Difficulty: {currentDifficulty}");
    }

    /// <summary>
    /// End the current game session and record the score
    /// </summary>
    public void EndGameSession(int finalScore)
    {
        if (string.IsNullOrEmpty(currentPlayerName))
        {
            Debug.LogWarning("ScoreManager: No current player set! Cannot record score.");
            return;
        }

        float playTime = Time.time - gameStartTime;
        AddScore(currentPlayerName, finalScore, currentMapName, currentDifficulty, playTime);
        
        // Clear session data
        currentPlayerName = "";
        currentMapName = "";
        currentDifficulty = "";
        gameStartTime = 0f;
    }

    private string GetDifficultyName(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "0": return "Beginner";
            case "1": return "Intermediate";
            case "2": return "Advanced";
            default: return difficulty;
        }
    }

    #endregion

    #region Score Management

    /// <summary>
    /// Add a new score entry
    /// </summary>
    public void AddScore(string playerName, int score, string mapName, string difficulty, float playTime)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("ScoreManager: Cannot add score with empty player name!");
            return;
        }

        GameScoreEntry newEntry = new GameScoreEntry(playerName, score, mapName, difficulty, playTime);
        scoreData.scores.Add(newEntry);

        // Sort scores by highest first
        scoreData.scores = scoreData.scores.OrderByDescending(s => s.score).ToList();

        // Limit the number of stored scores
        if (scoreData.scores.Count > maxScoreEntries)
        {
            scoreData.scores = scoreData.scores.Take(maxScoreEntries).ToList();
        }

        SaveScores();
        Debug.Log($"ScoreManager: Added score - {playerName}: {score} points on {mapName} ({difficulty})");
    }

    /// <summary>
    /// Get all scores sorted by highest first
    /// </summary>
    public List<GameScoreEntry> GetAllScores()
    {
        return scoreData.scores.OrderByDescending(s => s.score).ToList();
    }

    /// <summary>
    /// Get top N scores
    /// </summary>
    public List<GameScoreEntry> GetTopScores(int count = 10)
    {
        return scoreData.scores.OrderByDescending(s => s.score).Take(count).ToList();
    }

    /// <summary>
    /// Get scores for a specific map
    /// </summary>
    public List<GameScoreEntry> GetScoresForMap(string mapName)
    {
        return scoreData.scores.Where(s => s.mapName.Equals(mapName, StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.score).ToList();
    }

    /// <summary>
    /// Get scores for a specific player
    /// </summary>
    public List<GameScoreEntry> GetPlayerScores(string playerName)
    {
        return scoreData.scores.Where(s => s.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.score).ToList();
    }

    /// <summary>
    /// Get player's best score
    /// </summary>
    public GameScoreEntry GetPlayerBestScore(string playerName)
    {
        return scoreData.scores.Where(s => s.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.score).FirstOrDefault();
    }

    /// <summary>
    /// Clear all scores (use with caution!)
    /// </summary>
    public void ClearAllScores()
    {
        scoreData.scores.Clear();
        SaveScores();
        Debug.Log("ScoreManager: All scores cleared!");
    }

    #endregion

    #region File Operations

    private void LoadScores()
    {
        try
        {
            if (File.Exists(scoreFilePath))
            {
                string jsonData = File.ReadAllText(scoreFilePath);
                scoreData = JsonUtility.FromJson<GameScoreData>(jsonData);
                
                if (scoreData == null)
                {
                    scoreData = new GameScoreData();
                }
                
                Debug.Log($"ScoreManager: Loaded {scoreData.scores.Count} scores from file");
            }
            else
            {
                scoreData = new GameScoreData();
                Debug.Log("ScoreManager: No existing score file found. Created new score data.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ScoreManager: Error loading scores - {e.Message}");
            scoreData = new GameScoreData();
        }
    }

    private void SaveScores()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(scoreData, true);
            File.WriteAllText(scoreFilePath, jsonData);
            Debug.Log($"ScoreManager: Saved {scoreData.scores.Count} scores to file");
        }
        catch (Exception e)
        {
            Debug.LogError($"ScoreManager: Error saving scores - {e.Message}");
        }
    }

    #endregion

    #region Debug & Utility

    /// <summary>
    /// Add some sample scores for testing
    /// </summary>
    [ContextMenu("Add Sample Scores")]
    public void AddSampleScores()
    {
        AddScore("Harry Potter", 1500, "DungeonsScene", "Advanced", 180f);
        AddScore("Hermione Granger", 1800, "ChamberOfSecretsScene", "Advanced", 150f);
        AddScore("Ron Weasley", 1200, "DungeonsScene", "Intermediate", 200f);
        AddScore("Draco Malfoy", 1650, "ChamberOfSecretsScene", "Advanced", 170f);
        AddScore("Luna Lovegood", 1400, "DungeonsScene", "Beginner", 240f);
        
        Debug.Log("ScoreManager: Added sample scores for testing");
    }

    /// <summary>
    /// Print all scores to console for debugging
    /// </summary>
    [ContextMenu("Print All Scores")]
    public void PrintAllScores()
    {
        Debug.Log("=== ALL SCORES ===");
        foreach (var score in GetAllScores())
        {
            Debug.Log($"{score.playerName}: {score.score} pts ({score.mapName} - {score.difficulty}) [{score.dateTime}]");
        }
    }

    #endregion
}
