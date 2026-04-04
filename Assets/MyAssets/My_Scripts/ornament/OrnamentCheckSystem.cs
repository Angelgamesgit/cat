using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class OrnamentCheckSystem : MonoBehaviour
{
    List<Ornament> ornaments = new List<Ornament>();

    void Awake()
    {
        ornaments.AddRange(FindObjectsOfType<Ornament>());
        //プレイヤーデータのディクショナリにあるオーナメントの保存時間を取り出す
        //オーナメント側のチェックを走らせる計算を行う
        foreach (Ornament ornament in ornaments)
        {
            ornament.CheckItem();
        }
    }
   
}
