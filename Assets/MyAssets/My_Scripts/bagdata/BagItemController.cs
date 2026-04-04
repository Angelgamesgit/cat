using UnityEngine;

public class BagItemController : MonoBehaviour
{
    Rigidbody2D rb;

    bool dropped = false;

    public float moveSpeed = 10f;

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

        rb.gravityScale = 1;

        Debug.Log("Item Dropped");
    }
}