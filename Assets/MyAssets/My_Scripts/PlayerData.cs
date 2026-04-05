using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public GameObject CatPrefab;
    public List<CatData> catFound;
    //発見済みのスフィアのリスト
    public List<SphereSpec> FindSphereList;

    public SphereSpec currentSphereSpec;


    public enum BagType
    {
        Normal,
        Big,
        Hot,
        Speed,
        Lucky
    }
public BagType currentBagType;
    [ContextMenu("SavePlayerData")]
    public void Save()
    {
        ES3.Save<PlayerData>(name, this);
    }

    [ContextMenu("LoadPlayerdata")]
    public void Load()
    {
       
    }

    //
    public void AddItem(

    )
    {
     
        Save();
    }
    public void RemoveItem()
    {
        {
            Debug.Log("NoItem");
        }
    }

}
