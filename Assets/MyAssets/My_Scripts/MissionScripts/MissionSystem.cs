using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSystem : MonoBehaviour
{
    [SerializeField]
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    MissionData currentmissionData; //現在のミッションのデータを格納する変数
    public static MissionSystem missionSystem; //ミッション選択システムの情報を取得するための変数
    //ミッションを選択するシステムのスクリプト
    public enum MissonCrearRank
    {
        S,
        A,
        B,
        C,
        D
    }
    void Start()
    {
        missionSystem = this;
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
        currentmissionData = selectedMission; //現在のミッションのデータを更新する
    }

public void MissionStart()
    {
        //ミッションを開始するシステムの関数
        GetComponent<MissionUISystem>().StartMissionUI(); //ミッションUIを開始する
    }
    public void MissionClearCheck(CatData catData)
    {
        //ミッションをクリアするかどうかをチェックするシステムの関数
     if (currentmissionData.catData != catData)
        {
            //ミッションをクリアする処理をここに記述
            MissionClear(MissonCrearRank.S); //仮でSランクでクリアする
        }
     else
        {
            //ミッションを失敗する処理をここに記述
            MissionFail();
        }
    }
    public void MissionClear(MissonCrearRank missonCrearRank)
    {
        switch (missonCrearRank)
        {
            case MissonCrearRank.S:
                Debug.Log("ミッションをSランクでクリアしました！");
                break;
            case MissonCrearRank.A:
                Debug.Log("ミッションをAランクでクリアしました！");
                break;
            case MissonCrearRank.B:
                Debug.Log("ミッションをBランクでクリアしました！");
                break;
            case MissonCrearRank.C:
                Debug.Log("ミッションをCランクでクリアしました！");
                break;
            case MissonCrearRank.D:
                Debug.Log("ミッションをDランクでクリアしました！");
                break;
        }
        //ミッションをクリアするシステムの関数
        GetComponent<MissionUISystem>().ClearMissionUI(); //ミッションUIをクリアする
    }

public void MissionFail()
    {
        //ミッションを失敗するシステムの関数
        GetComponent<MissionUISystem>().HideMissionUI(); //ミッションUIを非表示にする
    }
}
