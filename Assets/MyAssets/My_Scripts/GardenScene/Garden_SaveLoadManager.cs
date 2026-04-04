
using UnityEngine;
using System.Collections.Generic;
// セーブ・ロード用のデータ構造
[System.Serializable]
public class Garden_ObjectSaveData
{
    public Garden_ItemSO ItemSO; 
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}




public class Garden_SaveLoadManager : MonoBehaviour
{
    [Tooltip("配置されたオブジェクトが子として追加される親Transform。Garden_EditorManagerから設定されます。")]
    public Transform objectParent;
    [HideInInspector] public List<Garden_ItemSO> availableGardenItems;

    private string _saveFileName = "MyGardenData_v5.es3"; // バージョンアップ
    private string _saveDataKey = "GardenState_v5";   // バージョンアップ
    public List<Garden_ObjectSaveData> placedObjects = new List<Garden_ObjectSaveData>();
    public void Save(List<Garden_ObjectSaveData> placedObjectDataList)
    {
        placedObjects = placedObjectDataList;
        ES3.Save(_saveDataKey, placedObjects, _saveFileName);
        Debug.Log($"庭データをEasy Saveで保存しました: {ES3.GetFiles(_saveFileName)} (Key: {_saveDataKey})");
    }

    public List<Garden_ObjectSaveData> Load()
    {
        if (!ES3.FileExists(_saveFileName) || !ES3.KeyExists(_saveDataKey, _saveFileName))
        {
            Debug.LogWarning($"セーブファイル '{_saveFileName}' またはキー '{_saveDataKey}' が見つかりません。新しいデータで開始します。");
            return null;
        }

        placedObjects = ES3.Load(_saveDataKey, _saveFileName, new List<Garden_ObjectSaveData>());
        Debug.Log($"庭データをEasy Saveで読み込みました: {_saveFileName}");
        return placedObjects;
    }

    public GameObject FindPrefabByItem(Garden_ItemSO item) // メソッド名変更
    {
        if (availableGardenItems == null)
        {
            Debug.LogError("SaveLoadManager: availableGardenItemsが設定されていません。");
            return null;
        }
        foreach (Garden_ItemSO itemSO in availableGardenItems)
        {
            if (itemSO != null && itemSO == item) // itemSO.nameで比較
            {
                return itemSO.prefab;
            }
        }
        Debug.LogWarning($"Prefab for item name '{name}' not found in availableGardenItems list.");
        return null;
    }
    
    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        if (ES3.FileExists(_saveFileName))
        {
            ES3.DeleteFile(_saveFileName);
            Debug.Log($"セーブファイル '{_saveFileName}' を削除しました。");
        }
        else
        {
            Debug.LogWarning($"セーブファイル '{_saveFileName}' は存在しません。削除できませんでした。");
        }
    }
}