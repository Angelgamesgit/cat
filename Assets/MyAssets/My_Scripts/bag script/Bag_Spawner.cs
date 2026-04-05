using UnityEngine;

public class Bag_Spawner : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject fruitPrefab;

    public bool TrySpawn(Bag_ItemData item)
{
    GameObject obj = Instantiate(
        fruitPrefab,
        spawnPoint.position,
        Quaternion.identity,
        transform);

    Bag_Item bagItem = obj.GetComponent<Bag_Item>();
    bagItem.Initialize(item);

    obj.AddComponent<Bag_ItemController>(); // ←追加

    return true;
}
}