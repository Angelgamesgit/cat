using UnityEngine;

public static class SuikaEventBridge
{
    public static System.Action<int> OnMiniGameFinished;

    public static void NotifyMiniGameEnd(int score)
    {
        OnMiniGameFinished?.Invoke(score);
    }
}