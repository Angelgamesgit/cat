using UnityEngine;
using ES3Types;


[CreateAssetMenu(fileName = "KitchenItem", menuName = "Scriptable Objects/KitchenItem")]
public class KitchenItem : ScriptableObject
{
    public Sprite icon;

    //所持数
    public int count;
    public void Save()
    {
        ES3.Save<int>("kitchenItem" + name, count);
    }

    public void Load()
    {
        count = ES3.Load<int>("kitchenItem" + name, 0);
    }


}
