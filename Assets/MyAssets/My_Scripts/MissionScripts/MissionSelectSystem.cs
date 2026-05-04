using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSelectSystem : MonoBehaviour
{
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    
    //ミッションを選択するシステムのスクリプト
    void Start()
    {
        MissionSelect();
    }
    /// <summary>
    /// ミッションを選択するシステムの関数
    /// </summary>
    void MissionSelect()
    {
        int randomIndex = Random.Range(0, missionDatas.Length); //ミッションのデータをランダムで選択する
        MissionData selectedMission = missionDatas[randomIndex];

        //選択されたミッションのデータを使用して、UIに表示するなどの処理を行う
    


    }
}
