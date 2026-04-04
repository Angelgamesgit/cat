using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager I;

    [Header("References")]
    public InventoryGrid Grid;
    public RectTransform ItemRoot;
    public InventoryItemUI ItemPrefab;
    public CombineDatabase CombineDB;
    public CombineDialog CombineDialog;

    [Header("Grid UI")]
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 gridStartPos;

    void Awake()
    {
        I = this;
    }

    public Vector2 GridToUI(Vector2Int gridPos)
    {
        return gridStartPos + new Vector2(
            gridPos.x * cellSize.x,
            -gridPos.y * cellSize.y
        );
    }

    public Vector2Int ScreenToGrid(Vector3 screenPos)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ItemRoot, screenPos, null, out local
        );

        int x = Mathf.FloorToInt((local.x - gridStartPos.x) / cellSize.x);
        int y = Mathf.FloorToInt((gridStartPos.y - local.y) / cellSize.y);

        return new Vector2Int(x, y);
    }

    public void SpawnItem(Item data)
    {
        var item = Instantiate(ItemPrefab, ItemRoot);
        item.Init(data);
        item.transform.localPosition = Vector3.zero;
    }

    public void TryPlaceOrDrop(InventoryItemUI item)
    {
        Vector2Int pos = ScreenToGrid(item.transform.position);

        if (Grid.CanPlace(item, pos))
        {
            Grid.Place(item, pos);
            CheckCombine(item);
        }
        else
        {
            Destroy(item.gameObject); // 捨てる
        }
    }

    void CheckCombine(InventoryItemUI placed)
    {
        var others = Grid.GetOverlappedItems(placed);

        foreach (var o in others)
        {
            var result = CombineDB.GetResult(placed.Data, o.Data);
            if (result != null)
            {
                CombineDialog.Show(
                    placed, o, result,
                    () => DoCombine(placed, o, result)
                );
                return;
            }
        }
    }

    void DoCombine(InventoryItemUI a, InventoryItemUI b, Item result)
    {
        Grid.Remove(a);
        Grid.Remove(b);
        Destroy(a.gameObject);
        Destroy(b.gameObject);

        SpawnItem(result);
    }
}
