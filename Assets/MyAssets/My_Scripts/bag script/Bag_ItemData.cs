using UnityEngine;

[CreateAssetMenu(menuName = "CityGame/BagItemData")]
public class Bag_ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public float radius;
    public Bag_ItemData next; // 合体進化先
}