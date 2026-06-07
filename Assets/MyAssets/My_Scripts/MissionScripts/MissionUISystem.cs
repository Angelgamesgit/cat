
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using TMPro;
public class MissionUISystem : MonoBehaviour
{
    CatPlayer catPlayer; //プレイヤーの情報を取得するための変数
    GameSystem gameSystem; //ゲームシステムの情報を取得するための変数

    [SerializeField]
    GameObject missionUI; //ミッションUIのゲームオブジェクトを格納する変数
[SerializeField]
    Image foodIcon; //食料アイコンを格納する変数
    [SerializeField]
    Image catIcon; //猫アイコンを格納する変数
    [SerializeField]
    TMP_Text mission_titleText; //ミッションのタイトルを格納する変数
    [SerializeField]
    TMP_Text descriptionText; //ミッションの説明を格納する変数
    [SerializeField]
    TMP_Text cookPatternText; //ミッションの大成功料理のパターンを格納する変数
    [SerializeField]
    GameObject catInfo_Panel; //猫の情報を表示するUIのゲームオブジェクトを格納する変数
    [SerializeField]
    TMP_Text catInfo_TitleText; //猫の情報を表示するUIのタイトルテキストを格納する変数
    [SerializeField]
    TMP_Text catInfo_DescriptionText; //猫の情報を表示するUIの説明テキストを格納する変数
    [SerializeField]
    TMP_Text catInfo_CookPatternText; //猫の情報を表示するUIのクックパターンテキストを格納する変数
        [SerializeField]
    GameObject catInfo_missionClear_Button; //猫の情報UIのミッションクリアボタンを格納する変数
    [SerializeField]
    Image catInfo_Icon; ///猫の情報を表示するUIのアイコンを格納する変数
    [SerializeField]
    GameObject missionClear_Panel; //ミッションをクリアしたときに表示するUIのゲームオブジェクトを格納する変数
    [SerializeField]
    GameObject missionFailed_Panel; //ミッションを失敗したときに表示するUIのゲームオブジェクトを格納する変数

    private void Awake()
    {
    }

    public void UpdateMissionUI(MissionData missionData)
    {
        // ミッションデータを使用してUIを更新する処理をここに記述
        // 例: ミッションのタイトル、説明、アイコンなどをUIに反映させる
    }

    public void ShowMissionUI(MissionData missionData)
    {
        if (GameSystem.missionState != GameSystem.MissionState.None)return; // ミッションが開始されていない場合はUIを表示しない
        // ミッションUIを表示する処理をここに記述
        UISystem.Instance.Panel_Open(missionUI.GetComponent<RectTransform>()); // ミッションUIを開く
        foodIcon.sprite = missionData.foodData.foodIcon; // ミッションの食料アイコンを設定
        catIcon.sprite = missionData.catData.catIcon; // ミッションの猫アイコンを設定
    }
    public void HideMissionUI()
    {
        // ミッションUIを非表示にする処理をここに記述
        UISystem.Instance.Panel_Close(missionUI.GetComponent<RectTransform>()); // ミッションUIを開く
    }

public void ShowCatInfoUI(CatData cat)
    {
        Debug.Log("ShowCatInfoUI");
        // 猫の情報をUIに表示する処理をここに記述
       UISystem.Instance.Panel_Open(catInfo_Panel.GetComponent<RectTransform>()); // 猫の情報UIを開く
        MissionSystem.missionSystem.info_CatData = cat; //ミッションシステムの猫のデータを更新する
        catInfo_Icon.sprite = cat.catIcon; // 猫のアイコンを設定する

        //ミッションが設定されていないときはミッションクリアボタンをオフにする
        if (MissionSystem.missionSystem.isMissionCurrentlyActive == false)
        {
            catInfo_missionClear_Button.SetActive(false);
        }
        else
        {
            catInfo_missionClear_Button.SetActive(true);
        }
    }
    public void HideCatInfoUI()
    {
        // 猫の情報をUIから非表示にする処理をここに記述
        UISystem.Instance.Panel_Close(catInfo_Panel.GetComponent<RectTransform>()); // 猫の情報UIを閉じる
    }

    public void StartMissionUI()
    {
        // ミッションUIを開始する処理をここに記述
        GameSystem.missionState = GameSystem.MissionState.Play;
        HideMissionUI(); // ミッションUIを非表示にする
    }

    public void Mission_StartClearCheck()
    {
        Debug.Log("Mission_StartClearCheck");
        MissionSystem.missionSystem.MissionClearCheck(); //ミッションをクリアするかどうかをチェックするシステムの関数を呼び出す
        HideCatInfoUI(); // 猫の情報UIを非表示にする
        Debug.Log("Mission_StartClearCheck_End");
    }
    public void ShowMissionClearUI()
    {
        // ミッションをクリアしたときに表示するUIを表示する処理をここに記述
        UISystem.Instance.Panel_Close(missionUI.GetComponent<RectTransform>()); // ミッションUIを閉じる
        UISystem.Instance.Panel_Open(missionClear_Panel.GetComponent<RectTransform>()); // ミッションクリアUIを開く
    }
    public void ShowMissionFailedUI()
    {
        UISystem.Instance.Panel_Close(missionUI.GetComponent<RectTransform>()); // ミッションUIを閉じる
        // ミッションを失敗したときに表示するUIを表示する処理をここに記述
        UISystem.Instance.Panel_Open(missionFailed_Panel.GetComponent<RectTransform>()); // ミッション失敗UIを開く
    }
}
