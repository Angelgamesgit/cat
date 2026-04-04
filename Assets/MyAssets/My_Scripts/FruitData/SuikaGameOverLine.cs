using UnityEngine;

public class SuikaGameOverLine : MonoBehaviour
{
    public float limitY;

    public System.Action OnGameOver;

    void Update()
    {
        foreach (var fruit in FindObjectsOfType<SuikaFruit>())
        {
            if (fruit.transform.position.y > limitY)
            {
                OnGameOver?.Invoke();
                enabled = false;
                break;
            }
        }
    }
}