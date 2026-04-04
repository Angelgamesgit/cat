using UnityEngine;

[CreateAssetMenu(menuName = "CityGame/BagItemData")]
public class BagItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public float radius;
    public BagItemData next; // 合体進化先
}