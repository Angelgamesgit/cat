using UnityEngine;

/// <summary>
/// プレイヤー（または指定ターゲット）が通過した場所に光を残します。
/// 一定距離移動するたびにLightSourceを生成します。
/// </summary>
public class PlayerPathLight : MonoBehaviour
{
    [Header("追従対象")]
    [Tooltip("光を生成する基準となるオブジェクト。未設定の場合はアタッチされたオブジェクト自身を追従します。")]
    [SerializeField]
    private Transform targetToTrack;

    [Header("光源")]
    [Tooltip("プレイヤーが通過した場所に生成する光")]
    [SerializeField]
    private LightSource lightPrefab;

    [Tooltip("光源を生成する間隔")]
    [SerializeField, Min(0.1f)]
    private float spawnDistance = 0.5f;

    [Header("生成位置")]
    [Tooltip("足元からの高さ方向（ターゲットの上方向）へのオフセット")]
    [SerializeField]
    private float heightOffset = 0.1f;

    [Header("光の種類")]
    [SerializeField]
    private LightType lightType = LightType.Small;

    [Header("光源制限")]
    [Tooltip("過去の光を残す最大数")]
    [SerializeField, Min(1)]
    private int maxLightCount = 100;

    private Vector3 lastLightPosition;

    private void Start()
    {
        // ターゲットが指定されていない場合はアタッチ先をターゲットにする
        if (targetToTrack == null)
        {
            targetToTrack = transform;
        }

        lastLightPosition = targetToTrack.position;
        CreateLight(lastLightPosition);
    }

    private void Update()
    {
        CheckDistance();
    }

    /// <summary>
    /// 前回光源を生成した位置から
    /// 一定距離移動したか確認します。
    /// </summary>
    private void CheckDistance()
    {
        if (targetToTrack == null) return;

        Vector3 currentPosition = targetToTrack.position;

        // 球体マップに対応するため、Y軸無視（difference.y = 0f）を廃止し純粋な3D空間の距離を取得
        float distance = Vector3.Distance(currentPosition, lastLightPosition);

        if (distance < spawnDistance)
        {
            return;
        }

        CreateLight(currentPosition);
        lastLightPosition = currentPosition;
    }

    /// <summary>
    /// 光を生成します。
    /// </summary>
    private void CreateLight(Vector3 position)
    {
        if (lightPrefab == null)
        {
            Debug.LogError("PlayerPathLight: LightPrefabが設定されていません。");
            return;
        }

        // 固定のY軸加算ではなく、ターゲットの上方向（球体の外側方向）へオフセットを適用
        position += targetToTrack.up * heightOffset;

        // 光源も球体表面の角度に合わせるため、ターゲットの回転を適用して生成
        LightSource light = Instantiate(lightPrefab, position, targetToTrack.rotation);

        light.SetLightType(lightType);

        // 古い光源を整理
        LimitLightCount();
    }

    /// <summary>
    /// 光源が増えすぎないようにします。
    /// </summary>
    private void LimitLightCount()
    {
        DarknessManager manager = FindFirstObjectByType<DarknessManager>();

        if (manager == null)
        {
            return;
        }

        while (manager.LightSources.Count > maxLightCount)
        {
            LightSource oldest = manager.LightSources[0];

            if (oldest == null)
            {
                break;
            }

            Destroy(oldest.gameObject);
        }
    }
}