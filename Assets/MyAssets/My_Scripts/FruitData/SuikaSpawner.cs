using UnityEngine;

public class SuikaSpawner : MonoBehaviour
{
    public Transform spawnAnchor;
    public GameObject fruitPrefab;

    SuikaFruitData next;

    public void SetNext(SuikaFruitData data)
    {
        next = data;
    }

    public void Spawn(Vector3 worldPos)
    {
        Vector3 pos = worldPos;
        pos.y = spawnAnchor.position.y;

        GameObject obj = Instantiate(fruitPrefab, pos, Quaternion.identity, transform);
        SuikaFruit fruit = obj.GetComponent<SuikaFruit>();
        fruit.Initialize(next);
    }
}