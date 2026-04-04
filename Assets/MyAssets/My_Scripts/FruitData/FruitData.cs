using UnityEngine;

[CreateAssetMenu(menuName = "Suika/FruitData")]
public class FruitData : ScriptableObject
{
    public string fruitName;
    public Sprite sprite;
    public int score;
    public float radius;
    public FruitData nextFruit;
}