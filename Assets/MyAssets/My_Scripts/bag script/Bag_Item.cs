using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bag_Item : MonoBehaviour
{
    public Bag_ItemData Data { get; private set; }

    bool merged;

    Rigidbody2D rb;

    bool dropped = false;

    public float moveSpeed;

  void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 最初は落とさない
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (dropped) return;

        MoveHorizontal();

        if (ReleaseInput())
        {
            Drop();
        }
    }

    void MoveHorizontal()
    {
        float inputX = 0;

#if UNITY_EDITOR || UNITY_STANDALONE
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        inputX = mouse.x - transform.position.x;
#else
        if (Input.touchCount > 0)
        {
            Vector3 touch = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            inputX = touch.x - transform.position.x;
        }
#endif

        Vector3 pos = transform.position;
        pos.x += inputX * moveSpeed * Time.deltaTime;
        transform.position = pos;
    }

    bool ReleaseInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonUp(0);
#else
        if (Input.touchCount == 0) return false;

        return Input.GetTouch(0).phase == TouchPhase.Ended;
#endif
    }

    void Drop()
    {
        dropped = true;

        rb.gravityScale = moveSpeed;

        Debug.Log("Item Dropped");
    }
    public void Initialize(Bag_ItemData itemData)
    {
        Data = itemData;

        GetComponent<Image>().sprite = itemData.itemSprite;
        GetComponent<CircleCollider2D>().radius = itemData.radius;

        transform.localScale = Vector3.one * itemData.radius * 2f;

        merged = false;

        Debug.Log("[BagItem] Spawn " + itemData.itemName);
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