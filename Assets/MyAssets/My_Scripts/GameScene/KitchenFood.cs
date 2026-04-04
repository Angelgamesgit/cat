using UnityEngine;

[CreateAssetMenu(fileName = "KitchenFood", menuName = "Scriptable Objects/KitchenFood")]
public class KitchenFood : ScriptableObject
{
    //
    public Sprite icon;
    //0でもOK 
    public float Cooktime;
    //入ってなくてもOK 
    public KitchenItem[] ingredients;
    float preparationTime;
}
