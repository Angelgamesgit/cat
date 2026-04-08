using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bag_Item : MonoBehaviour
{
    public Bag_ItemData Data { get; private set; }

    bool merged;

    public void Initialize(Bag_ItemData data)
    {
        Data = data;

        GetComponent<SpriteRenderer>().sprite = data.icon;
        GetComponent<CircleCollider2D>().radius = data.radius;

        transform.localScale = Vector3.one * data.radius * 2f;

        merged = false;

        Debug.Log("[BagItem] Spawn " + data.itemName);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (merged) return;

        Bag_Item other = collision.gameObject.GetComponent<Bag_Item>();

        if (other == null) return;
        if (other.Data != Data) return;
        if (other.merged) return;

        Debug.Log("[BagItem] Merge " + Data.itemName);

        Bag_MergeSystem.Instance.TryMerge(this, other);
    }

    public void LockMerge()
    {
        merged = true;
    }
    //動きが停止した時にほかのスクリプトに通知する関数
    public void NotifyStopped()
    {
        Debug.Log("[BagItem] Stopped : " + Data.itemName);
        
    }
}