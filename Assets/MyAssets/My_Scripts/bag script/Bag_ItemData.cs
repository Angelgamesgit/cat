using UnityEngine;

[CreateAssetMenu(menuName = "CityGame/BagItemData")]
public class Bag_ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public float radius;
    public Bag_ItemData next; // 合体進化先
}