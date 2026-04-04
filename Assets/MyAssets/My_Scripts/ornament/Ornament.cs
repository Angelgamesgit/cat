using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;

public class Ornament : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //時間経過でアイテムをが生成される
    //アイテムが取れるUIのシステムを呼び出す
    //種類がある
    public enum OrnamentType
    {
        star,
        heart,
        diamond,
        flower,
        musicnote,
    }

    public OrnamentType ornamentType;
    public Item setItem;

    bool isReadyToGetItem = false;
  
    public float createInterval = 5f;
    public long lastFalseUnixTime;
      /// <summary>
    /// アイテム生成可能か判定
    /// </summary>
    /// プレイヤーがインタラクトした時とゲームの起動時に呼び出し
    public void CheckItem()
    {
        if (isReadyToGetItem) return;
        PlayerData playerData = FindFirstObjectByType<GameSystem>().playerData;
        lastFalseUnixTime = playerData.ornamentTimeData[this];
        long now = GetUnixTime();
        long elapsed = now - lastFalseUnixTime;
        if (elapsed >= createInterval)
        {
            CreateItem();
        }
    }

      long GetUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    void CreateItem()
    {
        isReadyToGetItem = true;
        Save();
        Debug.Log($"{name} Item Created");
        // アイテム生成処理
    }
    void Save()
    {
        //プレイヤーデータのセーブを行う
        //他に必要なものがあればここで保存する
        PlayerData data = FindFirstObjectByType<GameSystem>().playerData;
       if(data.ornamentTimeData.ContainsKey(this))
        {
            data.ornamentTimeData[this] = lastFalseUnixTime;
        }
        else
        {
            data.ornamentTimeData.Add(this, lastFalseUnixTime);
        }
        data.Save();
    }

   
    public void GetItem()
    {
        if (setItem == null) return;
        if(!isReadyToGetItem) return;
        Debug.Log("Ornament Get Item: " + setItem.itemType);
        PlayerData data = FindFirstObjectByType<GameSystem>().playerData;
        data.AddItem(setItem.itemType, 1);
        isReadyToGetItem = false;
        lastFalseUnixTime = GetUnixTime();
        Save();
        //管理システムにアイテムを取ったことを伝える
    }
    //プレイヤーが画面を開いている時だけ時間で計測を行う

    void Remove(PlayerData data)
    {
        //オーナメントが消えるときに呼び出す
        //プレイヤーデータのオーナメントの保存時間を削除する
        if (data.ornamentTimeData.ContainsKey(this))
        {
            data.ornamentTimeData.Remove(this);
            data.Save();
        }
        //演出を入れたい場合ここで行う
        Destroy(gameObject);
    }
}
