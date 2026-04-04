using UnityEngine;

[CreateAssetMenu(menuName = "MiniGame/Suika/FruitData")]
public class SuikaFruitData : ScriptableObject
{
    public string fruitName;
    public Sprite sprite;
    public int score;
    public float radius;
    public SuikaFruitData next;
}