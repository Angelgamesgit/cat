using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Fruit : MonoBehaviour
{
    public FruitData data;

    private Rigidbody2D rb;
    private bool isMerged = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(FruitData fruitData)
    {
        data = fruitData;
        GetComponent<SpriteRenderer>().sprite = data.sprite;
        GetComponent<CircleCollider2D>().radius = data.radius;
        transform.localScale = Vector3.one * data.radius * 2f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMerged) return;

        Fruit other = collision.gameObject.GetComponent<Fruit>();
        if (other != null && other.data == data && !other.isMerged)
        {
            MergeManager.Instance.TryMerge(this, other);
        }
    }

    public void MarkMerged()
    {
        isMerged = true;
    }
}