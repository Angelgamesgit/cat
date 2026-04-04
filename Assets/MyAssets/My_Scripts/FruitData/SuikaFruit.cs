using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class SuikaFruit : MonoBehaviour
{
    public SuikaFruitData Data { get; private set; }

    Rigidbody2D rb;
    bool merged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(SuikaFruitData data)
    {
        Data = data;
        GetComponent<SpriteRenderer>().sprite = data.sprite;
        GetComponent<CircleCollider2D>().radius = data.radius;
        transform.localScale = Vector3.one * data.radius * 2f;
        merged = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (merged) return;

        SuikaFruit other = collision.gameObject.GetComponent<SuikaFruit>();
        if (other == null) return;
        if (other.Data != Data) return;
        if (other.merged) return;

        SuikaMergeSystem.Instance.RequestMerge(this, other);
    }

    public void LockMerge()
    {
        merged = true;
    }
}