using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    public const int WIDTH = 6;
    public const int HEIGHT = 8;

    InventoryItemUI[,] grid = new InventoryItemUI[WIDTH, HEIGHT];

    public bool CanPlace(InventoryItemUI item, Vector2Int pos)
    {
        foreach (var b in item.Data.shape.blocks)
        {
            int x = pos.x + b.x;
            int y = pos.y + b.y;

            if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                return false;

            if (grid[x, y] != null)
                return false;
        }
        return true;
    }

    public void Place(InventoryItemUI item, Vector2Int pos)
    {
        foreach (var b in item.Data.shape.blocks)
        {
            grid[pos.x + b.x, pos.y + b.y] = item;
        }

        item.SetGridPosition(pos);
    }

    public void Remove(InventoryItemUI item)
    {
        for (int x = 0; x < WIDTH; x++)
        for (int y = 0; y < HEIGHT; y++)
            if (grid[x, y] == item)
                grid[x, y] = null;
    }

    public List<InventoryItemUI> GetOverlappedItems(InventoryItemUI item)
    {
        HashSet<InventoryItemUI> result = new HashSet<InventoryItemUI>();

        foreach (var b in item.Data.shape.blocks)
        {
            int x = item.GridPos.x + b.x;
            int y = item.GridPos.y + b.y;

            if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                continue;

            if (grid[x, y] != null && grid[x, y] != item)
                result.Add(grid[x, y]);
        }

        return new List<InventoryItemUI>(result);
    }
}
