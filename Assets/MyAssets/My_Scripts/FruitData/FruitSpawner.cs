using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    private FruitData nextFruit;

    void Start()
    {
        GenerateNext();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.y = spawnPoint.position.y;
            pos.z = 0;
            Spawn(pos);
        }
    }

    void Spawn(Vector3 position)
    {
        GameObject obj = Instantiate(GameManager.Instance.fruitPrefab, position, Quaternion.identity);
        Fruit fruit = obj.GetComponent<Fruit>();
        fruit.Initialize(nextFruit);

        GenerateNext();
    }

    void GenerateNext()
    {
        nextFruit = GameManager.Instance.GetRandomFruit();
    }
}