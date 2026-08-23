using UnityEngine;

/// <summary>
/// 道を塞いでいる障害物を管理するクラス。
/// 岩、草、木など、種類ごとに必要なタップ回数をInspectorから設定できます。
/// </summary>
public class ObstacleObject : MonoBehaviour
{
    [Header("障害物設定")]
    
    public Data_ObstacleObject data;

    [Header("破壊設定")]


    [Tooltip("タップされたときに表示するデバッグログ")]
    [SerializeField]
    private bool showDebugLog = false;

    private int currentTapCount;

    /// <summary>
    /// 残りのタップ回数。
    /// </summary>
    public int RemainingTapCount => currentTapCount;

    /// <summary>
    /// 必要なタップ回数。
    /// </summary>
    public int RequiredTapCount => data.requiredTapCount;

     /// <summary>
    /// 現在選択されている道具で障害物を叩きます。
    /// </summary>
    public void Hit(ToolType currentTool)
    {
        // 必要な道具と現在選択している道具が違う
        if (currentTool != data.requiredTool)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"{gameObject.name} は {data.requiredTool} が必要です。"
                );
            }

            return;
        }

        // すでに破壊済み
        if (currentTapCount <= 0)
        {
            return;
        }

        currentTapCount--;

        if (showDebugLog)
        {
            Debug.Log(
                $"{gameObject.name} を {currentTool} で叩きました。 " +
                $"残り {currentTapCount} 回"
            );
        }

        // 必要回数に到達
        if (currentTapCount <= 0)
        {
            Break();
        }
    }

    /// <summary>
    /// 障害物を破壊します。
    /// </summary>
    private void Break()
    {
        if (showDebugLog)
        {
            Debug.Log(
                $"{gameObject.name} を破壊しました。"
            );
        }

        // 今後ここに
        // ・破壊アニメーション
        // ・パーティクル
        // ・SE
        // ・道を開通させる処理
        // などを追加できます。

        Destroy(gameObject);
    }
}