using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void TryMerge(Fruit a, Fruit b)
    {
        if (a.data.nextFruit == null) return;

        a.MarkMerged();
        b.MarkMerged();

        Vector3 spawnPos = (a.transform.position + b.transform.position) / 2f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject newFruitObj = Instantiate(GameManager.Instance.fruitPrefab, spawnPos, Quaternion.identity);
        Fruit newFruit = newFruitObj.GetComponent<Fruit>();
        newFruit.Initialize(a.data.nextFruit);

        ScoreManager.Instance.AddScore(a.data.nextFruit.score);
    }
}