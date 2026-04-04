using UnityEngine;
using TMPro;

public class SuikaScoreSystem : MonoBehaviour
{
    public static SuikaScoreSystem Instance;

    public TextMeshProUGUI scoreText;

    int score;

    void Awake()
    {
        Instance = this;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
    }

    public int GetScore() => score;
}