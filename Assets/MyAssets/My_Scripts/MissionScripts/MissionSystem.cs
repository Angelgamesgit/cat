using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSelectSystem : MonoBehaviour
{
    [SerializeField]
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    public static MissionSelectSystem missionSelectSystem; //ミッション選択システムの情報を取得するための変数
    //ミッションを選択するシステムのスクリプト
    void Start()
    {
        missionSelectSystem = this;
    }
    /// <summary>
    /// ミッションを選択するシステムの関数
    /// </summary>
    public void MissionSelect()
    {
        int randomIndex = Random.Range(0, missionDatas.Length); //ミッションのデータをランダムで選択する
        MissionData selectedMission = missionDatas[randomIndex];
        //選択されたミッションのデータを使用して、UIに表示するなどの処理を行う
        GetComponent<MissionUISystem>().ShowMissionUI(selectedMission);
    }

public void MissionStart()
    {
        //ミッションを開始するシステムの関数
        GetComponent<MissionUISystem>().StartMissionUI(); //ミッションUIを開始する
    }
    public void MissionClear()
    {
        //ミッションをクリアするシステムの関数
        GetComponent<MissionUISystem>().ClearMissionUI(); //ミッションUIをクリアする
    }

public void MissionFail()
    {
        //ミッションを失敗するシステムの関数
        GetComponent<MissionUISystem>().HideMissionUI(); //ミッションUIを非表示にする
    }
}
