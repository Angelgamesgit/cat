using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSystem : MonoBehaviour
{
    [SerializeField]
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    MissionData currentMissionData; //現在のミッションのデータを格納する変数
    public static MissionSystem missionSystem; //ミッション選択システムの情報を取得するための変数
    public MissionUISystem missionUISystem; //ミッションUIシステムの情報を取得するための変数

    public bool isMissionCurrentlyActive; //ミッションが現在アクティブかどうかを格納する変数

    public CatData info_CatData; //猫のデータを格納する変数
    //ミッションを選択するシステムのスクリプト
    public enum MissonCrearRank
    {
        S,
        A,
        B,
        C,
        D
    }
    private void Awake()
    {
        if (missionSystem == null)
        {
            missionSystem = this; //ミッション選択システムの情報を取得するための変数にこのスクリプトを代入する
        }
        else
        {
            Destroy(gameObject); //すでにミッション選択システムの情報を取得するための変数にこのスクリプトが代入されている場合は、このスクリプトを破棄する
        }
        missionUISystem = GetComponent<MissionUISystem>(); //ミッションUIシステムの情報を取得するための変数にこのスクリプトのMissionUISystemコンポーネントを代入する
    isMissionCurrentlyActive = false; //ミッションが現在アクティブかどうかを格納する変数をfalseにする
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
        currentMissionData = selectedMission; //現在のミッションのデータを更新する
        isMissionCurrentlyActive = true; //ミッションが現在アクティブかどうかを格納する変数をtrueにする
    }

public void MissionStart()
    {
        //ミッションを開始するシステムの関数
        missionUISystem.StartMissionUI(); //ミッションUIを開始する
    }

    public void MissionClearCheck()
    {
        if (currentMissionData == null)
        {
            Debug.LogError("現在のミッションのデータが設定されていません。 仮使用でクリアになってます");
                MissionClear(MissonCrearRank.S); //仮でSランクでクリアする
            return;
        }
        //ミッションをクリアするかどうかをチェックするシステムの関数
    if (currentMissionData.catData != info_CatData)
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
        isMissionCurrentlyActive = false; //ミッションが現在アクティブかどうかを格納する変数をfalseにする
        switch (missonCrearRank)
        {
            case MissonCrearRank.S:
                Debug.Log("ミッションをSランクでクリア　処理を開始");
                break;
            case MissonCrearRank.A:
                Debug.Log("ミッションをAランクでクリア　処理を開始");
                break;
            case MissonCrearRank.B:
                Debug.Log("ミッションをBランクでクリア　処理を開始");
                break;
            case MissonCrearRank.C:
                Debug.Log("ミッションをCランクでクリア　処理を開始");
                break;
            case MissonCrearRank.D:
                Debug.Log("ミッションをDランクでクリア　処理を開始");
                break;
        }
        //ミッションをクリアするシステムの関数
        GetComponent<MissionUISystem>().ShowMissionClearUI(); //ミッションUIをクリアする
    }

public void MissionFail()
    {
        //ミッションを失敗するシステムの関数
        GetComponent<MissionUISystem>().ShowMissionFailedUI(); //ミッションUIを非表示にする
    }
}
