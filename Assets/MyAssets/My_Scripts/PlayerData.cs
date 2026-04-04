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

    public Dictionary<Item.ItemType, int> itemCount;
    
    public Dictionary<Ornament, long> ornamentTimeData;


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
        if (itemCount == null)
        {
            itemCount = new Dictionary<Item.ItemType, int>();
        }
        if (ornamentTimeData == null)
        {
            ornamentTimeData = new Dictionary<Ornament, long>();
        }
        ES3.Load<PlayerData>(name, this);
        Debug.Log("Load Complete");
    }

    //
    public void AddItem(Item.ItemType itemType, int num)
    {
        if (itemCount.ContainsKey(itemType))
        {
            itemCount[itemType] += num;
        }
        else
        {
            itemCount.Add(itemType, num);
            UnityEngine.Debug.Log("NewGetItem");
        }
        Save();
    }
    public void RemoveItem(Item.ItemType itemType)
    {
        if (itemCount.ContainsKey(itemType))
        {
            itemCount[itemType]--;
        }
        //含まれていない場合何かエラーがある
        else
        {
            Debug.Log("NoItem");
        }
    }

}
