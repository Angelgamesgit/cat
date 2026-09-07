using UnityEngine;

/// <summary>
/// プレイヤーが移動した場所を
/// SphereExplorationTextureへ記録する。
///
/// 明るい場所しか移動できない、という
/// 移動制限は一切行わない。
/// </summary>
public class PlayerExplorationTrail
    : MonoBehaviour
{
    [Header("Exploration System")]

    [SerializeField]
    private SphereExplorationTexture exploration;

    [Header("Trail Settings")]

    [Tooltip("この距離移動するたびに探索場所を追加")]
    [SerializeField]
    [Min(0.01f)]
    private float revealDistance = 0.2f;

    [Tooltip("移動中に追加する光の間隔")]
    [SerializeField]
    [Min(0.01f)]
    private float interpolationDistance = 0.1f;

    private Vector3 lastPosition;

    private bool initialized;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化。
    /// </summary>
    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        if (exploration == null)
        {
            exploration =
                FindFirstObjectByType<
                    SphereExplorationTexture>();
        }

        if (exploration == null)
        {
            Debug.LogError(
                "PlayerExplorationTrail : " +
                "SphereExplorationTextureが見つかりません。"
            );

            return;
        }

        lastPosition =
            transform.position;

        // ゲーム開始時の猫の位置も明るくする。
        exploration.RevealPlayerPosition(
            transform.position
        );

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        UpdateExploration();
    }

    /// <summary>
    /// 猫の移動を検出して探索領域を追加する。
    /// </summary>
    private void UpdateExploration()
    {
        Vector3 currentPosition =
            transform.position;

        float distance =
            Vector3.Distance(
                lastPosition,
                currentPosition
            );

        if (
            distance <
            revealDistance
        )
        {
            return;
        }

        RevealBetweenPositions(
            lastPosition,
            currentPosition
        );

        lastPosition =
            currentPosition;
    }

    /// <summary>
    /// 前回位置と現在位置の間を
    /// 途切れないように明るくする。
    /// </summary>
    private void RevealBetweenPositions(
        Vector3 start,
        Vector3 end)
    {
        float distance =
            Vector3.Distance(
                start,
                end
            );

        int count =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    distance /
                    interpolationDistance
                )
            );

        for (
            int i = 1;
            i <= count;
            i++
        )
        {
            float t =
                (float)i /
                count;

            Vector3 position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );

            exploration.RevealPlayerPosition(
                position
            );
        }
    }

    /// <summary>
    /// 外部から現在位置を
    /// 探索済みにする。
    /// </summary>
    public void RevealCurrentPosition()
    {
        if (exploration == null)
        {
            return;
        }

        exploration.RevealPlayerPosition(
            transform.position
        );

        lastPosition =
            transform.position;
    }

    /// <summary>
    /// GameSystem等から探索システムを設定する場合。
    /// </summary>
    public void SetExplorationSystem(
        SphereExplorationTexture system)
    {
        exploration =
            system;

        lastPosition =
            transform.position;

        if (exploration != null)
        {
            exploration.RevealPlayerPosition(
                transform.position
            );
        }
    }
}