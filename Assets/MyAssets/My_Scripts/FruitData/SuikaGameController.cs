using UnityEngine;

public class SuikaGameController : MonoBehaviour
{
    public SuikaFruitData[] fruits;
    public SuikaSpawner spawner;
    public SuikaGameOverLine gameOverLine;

    bool isPlaying;

    void Start()
    {
        gameOverLine.OnGameOver += EndGame;
    }

    public void StartGame()
    {
        isPlaying = true;
        SuikaScoreSystem.Instance.ResetScore();
        gameObject.SetActive(true);
    }

    public void EndGame()
    {
        isPlaying = false;
        Debug.Log("MiniGame End");
        SuikaEventBridge.NotifyMiniGameEnd(
            SuikaScoreSystem.Instance.GetScore());
    }

    public SuikaFruitData GetRandom()
    {
        return fruits[Random.Range(0, 5)];
    }
}