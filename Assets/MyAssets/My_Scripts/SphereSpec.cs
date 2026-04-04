using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereSpec", menuName = "Scriptable Objects/SphereSpec")]
public class SphereSpec : ScriptableObject
{

    public string sphereName;
    //見つける猫のデータ　見つけたかどうかはプレイヤー側で管理する
    public List<CatData> findCatsDate;
    public CatData lastCatData;
    public GameObject Sphere;
    public Sprite icon;

    public DaySphereSystem.DaySphereType[] dayType;

    public enum WeatherState
    {
        Sunny,Rainy,Snowy,SandWind
    }
    public WeatherState weatherState;
}
