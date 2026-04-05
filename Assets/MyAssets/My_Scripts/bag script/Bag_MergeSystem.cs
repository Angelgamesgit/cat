using UnityEngine;

public class Bag_MergeSystem : MonoBehaviour
{
    public static Bag_MergeSystem Instance;

    public GameObject fruitPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void TryMerge(Bag_Item a, Bag_Item b)
    {
        if (a.Data.next == null)
        {
            Debug.Log("[Merge] No Next Item");
            return;
        }

        Debug.Log("[Merge] " + a.Data.itemName + " -> " + a.Data.next.itemName);

        a.LockMerge();
        b.LockMerge();

        Vector3 pos = (a.transform.position + b.transform.position) * 0.5f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject obj = Instantiate(
            fruitPrefab,
            pos,
            Quaternion.identity,
            transform);

        obj.GetComponent<Bag_Item>().Initialize(a.Data.next);
    }
}