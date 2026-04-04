using UnityEngine;

public class BagMergeSystem : MonoBehaviour
{
    public static BagMergeSystem Instance;

    public GameObject fruitPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void TryMerge(BagItem a, BagItem b)
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

        obj.GetComponent<BagItem>().Initialize(a.Data.next);
    }
}