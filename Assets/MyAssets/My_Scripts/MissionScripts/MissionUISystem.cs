using UnityEngine;

public class MissionUISystem : MonoBehaviour
{
    public static MissionUISystem Instance { get; private set; }
    CatPlayer catPlayer; //プレイヤーの情報を取得するための変数
    GameSystem gameSystem; //ゲームシステムの情報を取得するための変数
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでUIを保持する場合
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は新しいものを破棄
        }
    }

    public void UpdateMissionUI(MissionData missionData)
    {
        // ミッションデータを使用してUIを更新する処理をここに記述
        // 例: ミッションのタイトル、説明、アイコンなどをUIに反映させる
    }

    public void ShowMissionUI()
    {
        // ミッションUIを表示する処理をここに記述
    }

    public void HideMissionUI()
    {
        // ミッションUIを非表示にする処理をここに記述
    }
    public void ClearMissionUI()
    {
        // ミッションUIをクリアする処理をここに記述
    }

    public void StatrtMissionUI()
    {
        // ミッションUIを開始する処理をここに記述
        GameSystem.missionState = GameSystem.MissionState.Play;
    }
}
