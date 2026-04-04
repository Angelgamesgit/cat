// === File: Garden_UndoAction.cs ===
// (プロジェクトフォルダ/Scripts/Actions/ などに作成してください)
using UnityEngine;
using System.Collections.Generic;

public enum UndoActionType
{
    ObjectPlaced,
    ObjectMovedRotated,
    ObjectRemoved,
    AllObjectsRemoved
}

[System.Serializable]
public class Garden_UndoAction
{
    public UndoActionType actionType;
    public Garden_ItemSO targetItem; // Garden_ItemSO.name を格納
    public int targetInstanceID;

    public Vector3 previousPosition;
    public Quaternion previousRotation;

    public Garden_ObjectSaveData objectData;
    public List<Garden_ObjectSaveData> multipleObjectsData;

    public Garden_UndoAction(GameObject placedObject, Garden_ItemSO item) // ObjectPlaced
    {
        this.actionType = UndoActionType.ObjectPlaced;
        this.targetInstanceID = placedObject.GetInstanceID();
        this.targetItem = item; // itemSO.name を受け取る
    }

    public Garden_UndoAction(GameObject target, Vector3 prevPos, Quaternion prevRot) // ObjectMovedRotated
    {
        this.actionType = UndoActionType.ObjectMovedRotated;
        this.targetInstanceID = target.GetInstanceID();
        
        this.targetItem = target.GetComponent<Garden_PlaceableObjectData>().itemSO; // ここも itemSO.name が入る
        this.previousPosition = prevPos;
        this.previousRotation = prevRot;
    }

    public Garden_UndoAction(Garden_ObjectSaveData removedObjectData) // ObjectRemoved
    {
        this.actionType = UndoActionType.ObjectRemoved;
        this.objectData = removedObjectData; // objectData.itemId は itemSO.name
        this.targetItem = removedObjectData.ItemSO;
    }

    public Garden_UndoAction(List<Garden_ObjectSaveData> removedObjectsList) // AllObjectsRemoved
    {
        this.actionType = UndoActionType.AllObjectsRemoved;
        this.multipleObjectsData = removedObjectsList;
    }
}