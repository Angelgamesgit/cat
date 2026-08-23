using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSystem : MonoBehaviour
{
    GameSystem gameSystem; //ゲームシステムの情報を取得するための変数
    [SerializeField]
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    MissionData currentMissionData; //現在のミッションのデータを格納する変数
    public static MissionSystem missionSystem; //ミッション選択システムの情報を取得するための変数
    public MissionUISystem missionUISystem; //ミッションUIシステムの情報を取得するための変数

    public bool isMissionCurrentlyActive; //ミッションが現在アクティブかどうかを格納する変数

    public CatData info_CatData; //猫のデータを格納する変数

    public float missionClearCount; //ミッションをクリアした回数を格納する変数
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
        gameSystem = GetComponent<GameSystem>(); //ゲームシステムの情報を取得するための変数にこのスクリプトのGameSystemコンポーネントを代入する
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
    if (currentMissionData.catData == info_CatData)
        {

            //ミッションをクリアする処理をここに記述
            /*
            料理の調理状態があっているか +1
            時間制限を満たしているか +1 ~ +3
            */
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
                Debug.Log("ミッションをSランクでクリア 処理を開始");
                break;
            case MissonCrearRank.A:
                Debug.Log("ミッションをAランクでクリア処理を開始");
                break;
            case MissonCrearRank.B:
                Debug.Log("ミッションをBランクでクリア処理を開始");
                break;
            case MissonCrearRank.C:
                Debug.Log("ミッションをCランクでクリア処理を開始");
                break;
            case MissonCrearRank.D:
                Debug.Log("ミッションをDランクでクリア処理を開始");
                break;
        }
        currentMissionData = null; //現在のミッションのデータをリセットする
        //ミッションをクリアするシステムの関数
        GetComponent<MissionUISystem>().ShowMissionClearUI(); //ミッションUIをクリアする
    }

public void MissionFail()
    {
        //ミッションを失敗するシステムの関数
        GetComponent<MissionUISystem>().ShowMissionFailedUI(); //ミッションUIを非表示にする
        Debug.Log("ミッションを失敗しました。 処理を開始");
    }

public void MissionReset()
    {
        //ミッションをリセットするシステムの関数
        currentMissionData = null; //現在のミッションのデータをリセットする
        isMissionCurrentlyActive = false; //ミッションが現在アクティブかどうかを格納する変数をfalseにする
    }

    public void MissionTimeSystem()
    {
        //ミッションの時間を管理するシステムの関数
        //ミッションが現在アクティブな場合
        if (isMissionCurrentlyActive && gameSystem.isPlaying)
        {
            //ミッションが現在アクティブな場合の処理をここに記述
            //例えば、ミッションの時間を減らすなどの処理を行う
            currentMissionData.missionTime -= Time.deltaTime; //仮でミッションの時間を減らす
        }
    }
}
