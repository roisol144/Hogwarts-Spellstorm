using UnityEngine;
using TMPro;

public class ScoreEntry : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void SetEntry(int rank, string playerName, int score)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (nameText != null) nameText.text = playerName;
        if (scoreText != null) scoreText.text = score.ToString();
    }
} 