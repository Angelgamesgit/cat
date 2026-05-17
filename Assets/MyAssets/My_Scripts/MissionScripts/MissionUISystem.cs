using Microsoft.Unity.VisualStudio.Editor;
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
    TMP_Text titleText; //ミッションのタイトルを格納する変数
    [SerializeField]
    TMP_Text descriptionText; //ミッションの説明を格納する変数
    [SerializeField]
    TMP_Text cookPatternText; //ミッションの大成功料理のパターンを格納する変数

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
    public void ClearMissionUI()
    {
        // ミッションUIをクリアする処理をここに記述
    }

    public void StartMissionUI()
    {
        // ミッションUIを開始する処理をここに記述
        GameSystem.missionState = GameSystem.MissionState.Play;
        HideMissionUI(); // ミッションUIを非表示にする
    }
}
