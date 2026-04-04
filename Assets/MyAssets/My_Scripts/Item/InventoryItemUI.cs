using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item Data { get; private set; }
    public Vector2Int GridPos { get; private set; }

    RectTransform rect;
    Canvas canvas;
    Image image;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();
    }

    public void Init(Item data)
    {
        Data = data;
        image.sprite = data.icon;
    }

    public void SetGridPosition(Vector2Int pos)
    {
        GridPos = pos;
        rect.anchoredPosition = InventoryManager.I.GridToUI(pos);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryManager.I.Grid.Remove(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position += (Vector3)eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryManager.I.TryPlaceOrDrop(this);
    }
}
