using UnityEngine;

public class BagSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject fruitPrefab;

    public bool TrySpawn(BagItemData item)
{
    GameObject obj = Instantiate(
        fruitPrefab,
        spawnPoint.position,
        Quaternion.identity,
        transform);

    BagItem bagItem = obj.GetComponent<BagItem>();
    bagItem.Initialize(item);

    obj.AddComponent<BagItemController>(); // ←追加

    return true;
}
}