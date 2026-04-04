// === File: Garden_ItemSO.cs ===
// (プロジェクトフォルダ/Scripts/Data/ などに作成してください)
// GardenItem ScriptableObjectアセットの作成方法:
// Projectウィンドウで右クリック > Create > Garden > Item SO
// アイテムIDは、このアセットのファイル名が自動的に使用されます。
using UnityEngine;
using ES3Types;
[CreateAssetMenu(fileName = "NewGardenItem", menuName = "Garden/Item SO")]
public class Garden_ItemSO : ScriptableObject
{
    // itemId フィールドは削除。ScriptableObject.name を IDとして使用します。

    [Tooltip("配置されるゲームオブジェクトのプレハブ。")]
    public GameObject prefab;
    [Tooltip("UIボタンなどに表示される名前。")]
    public string displayName;
    [Tooltip("このアイテムを手に入れるために必要なアイテムの個数。")]
    public int needNum;
    [Tooltip("このアイテムを手に入れるために必要なアイテムの種類。")]
    public Item.ItemType needItemType;
    [Tooltip("UIボタンに表示されるアイコン。")]
    public Sprite icon;
    [Tooltip("このアイテムの初期の総所持数。")]
    public int initialTotalPossession;

    string counttext = "Count";
    [ContextMenu("SaveItemData")]
    public void Save()
    {
        ES3.Save<int>(name + counttext, initialTotalPossession);
    }
    [ContextMenu("LoadItemData")]
    public void Load()
    {
        initialTotalPossession = ES3.Load<int>(name + counttext, 0);
    }
}



