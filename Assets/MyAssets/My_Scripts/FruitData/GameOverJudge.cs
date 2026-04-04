using UnityEngine;

public class GameOverJudge : MonoBehaviour
{
    public float lineY = 5f;

    void Update()
    {
        Fruit[] fruits = FindObjectsOfType<Fruit>();
        foreach (Fruit f in fruits)
        {
            if (f.transform.position.y > lineY)
            {
                GameManager.Instance.GameOver();
                break;
            }
        }
    }
}