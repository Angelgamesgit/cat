// ファイル名: FirstLaunchCleaner.cs
using UnityEngine;

/// <summary>
/// アプリケーションの初回起動時に一度だけ実行され、Easy Save 3のデータをすべて消去します。
/// これにより、再インストール時などに古いデータが残っている場合でも、クリーンな状態でゲームを開始できます。
/// </summary>
public class FirstLaunchCleaner : MonoBehaviour
{
    // 初回起動を判定するためのキー（一度設定されたら変更しないこと）
    private const string FirstLaunchCheckKey = "HasLaunchedGameBefore";

    void Awake()
    {
        // PlayerPrefsにキーが存在しない場合、初回起動とみなす
        if (!PlayerPrefs.HasKey(FirstLaunchCheckKey))
        {
            Debug.Log("アプリケーションの初回起動を検出しました。既存のセーブデータを消去します。");

            // Easy Save 3のデフォルトセーブファイル名
            string defaultSaveFile = "SaveFile.es3";

            // ファイルが存在すれば削除
            if (ES3.FileExists(defaultSaveFile))
            {
                ES3.DeleteFile(defaultSaveFile);
                Debug.Log($"Easy Save のデータ ({defaultSaveFile}) を消去しました。");
            }

            // 次回以降、この処理が実行されないようにキーを設定
            PlayerPrefs.SetInt(FirstLaunchCheckKey, 1);
            PlayerPrefs.Save(); // 変更をディスクに書き込む

            Debug.Log("初回起動処理が完了しました。");
        }
    }
}