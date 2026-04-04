using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BagItem : MonoBehaviour
{
    public BagItemData Data { get; private set; }

    bool merged;

    public void Initialize(BagItemData data)
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

        BagItem other = collision.gameObject.GetComponent<BagItem>();

        if (other == null) return;
        if (other.Data != Data) return;
        if (other.merged) return;

        Debug.Log("[BagItem] Merge " + Data.itemName);

        BagMergeSystem.Instance.TryMerge(this, other);
    }

    public void LockMerge()
    {
        merged = true;
    }
}