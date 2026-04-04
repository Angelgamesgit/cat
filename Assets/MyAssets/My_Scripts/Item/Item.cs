using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UI;
using DG.Tweening;
[CreateAssetMenu(fileName = "New Item")]
public class Item : ScriptableObject  
{
    public enum ItemType
    {
wood,
apple,
orange,
banana,
grape,

    }
    [System.Serializable]
public class ItemShape
{
    public int width;
    public int height;

    // 使用マス（左上原点）
    public Vector2Int[] blocks;
}
public bool canCombine;
public ItemShape shape;
    public ItemType itemType;
    public int width = 1;
    public int height = 1;
   
    
    public Sprite icon;

    [System.Serializable]
    public struct Recipe {
        public Item material;
        public Item result;
    }
    public List<Recipe> synthesisRecipes; // 合成レシピ
    [SerializeField]
    string itemdescription;
    [SerializeField]
    public int itemNumber;

    public List<Vector2Int> occupiedPoints = new List<Vector2Int> { new Vector2Int(0,0) };
   
    public string GetItemName()
    {
        return itemType.ToString();
    }
    public string GetItemDescription()
    {
        return itemdescription;
    }
  public int GetItemNumber()
    {
        return (int)itemType;
    }
}
