using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タッチした場所への光と、
/// 光が当たったオブジェクトを目的地へ変更するシステム。
///
/// 処理の流れ
/// 
/// タッチ
/// ↓
/// タッチ地点を取得
/// ↓
/// 光を生成
/// ↓
/// 光が当たっている対象を検索
/// ↓
/// 対象があれば目的地を対象オブジェクトへ変更
///
/// ※このスクリプト自身は猫を移動させない。
///   「目的地」を通知するだけ。
/// </summary>
public class LightTargetSystem : MonoBehaviour
{
    [Header("Light Settings")]

    [Tooltip("光が届く範囲")]
    [SerializeField]
    private float lightRadius;

    [Tooltip("光が当たった対象を検索するLayer")]
    [SerializeField]
    private LayerMask targetLayer;

    [Tooltip("同時に検出できる最大オブジェクト数")]
    [SerializeField]
    private int maxTargetCount;

    [Header("Target Selection")]

    [Tooltip("複数対象がある場合、最も近いものを目的地にする")]
    [SerializeField]
    private bool selectNearestTarget = true;

    [Tooltip("対象の中心位置を目的地として使用する")]
    [SerializeField]
    private bool useTargetCenter = true;

    [Header("Debug")]

    [SerializeField]
    private bool debugLog = true;

    [SerializeField]
    private bool drawDebugRadius = true;

    private Collider[] targetBuffer;

    private readonly List<Collider> detectedTargets =
        new List<Collider>();

    /// <summary>
    /// 現在選択されている目的地。
    /// </summary>
    public Transform CurrentTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// 最後にタッチしたワールド座標。
    /// </summary>
    public Vector3 LastTouchPosition
    {
        get;
        private set;
    }

    /// <summary>
    /// 光が当たったオブジェクトを目的地に変更した時に発生。
    /// </summary>
    public event System.Action<Transform> OnTargetChanged;

    /// <summary>
    /// タッチ地点を受け取る。
    ///
    /// GameSystemなどから呼び出す。
    /// </summary>
    public Transform TouchPosition(
        Vector3 worldPosition)
    {
        LastTouchPosition =
            worldPosition;

        // まずタッチ地点を通常の目的地にする。
        SetTarget(null);

        // 光が当たる範囲を検索。
        Transform target =
            FindTarget(worldPosition);

        // 対象が見つかった場合、
        // タッチ地点ではなく対象を目的地にする。
        if (target != null)
        {
            SetTarget(target);
        }

        return CurrentTarget;
    }

    /// <summary>
    /// 目的地を設定。
    /// </summary>
    private void SetTarget(
        Transform target)
    {
        CurrentTarget =
            target;

        OnTargetChanged?.Invoke(
            CurrentTarget
        );

        if (debugLog)
        {
            if (target != null)
            {
                Debug.Log(
                    $"[LightTargetSystem] " +
                    $"目的地変更 : {target.name}"
                );
            }
            else
            {
                Debug.Log(
                    "[LightTargetSystem] " +
                    "タッチ地点を目的地に設定"
                );
            }
        }
    }

    /// <summary>
    /// 指定位置に光を当てたとき、
    /// 光が当たる対象を検索する。
    /// </summary>
    private Transform FindTarget(
        Vector3 worldPosition)
    {
        detectedTargets.Clear();

        if (targetBuffer == null ||
            targetBuffer.Length != maxTargetCount)
        {
            targetBuffer =
                new Collider[maxTargetCount];
        }

        int hitCount =
            Physics.OverlapSphereNonAlloc(
                worldPosition,
                lightRadius,
                targetBuffer,
                targetLayer,
                QueryTriggerInteraction.Ignore
            );

        if (hitCount <= 0)
        {
            return null;
        }

        for (
            int i = 0;
            i < hitCount;
            i++
        )
        {
            Collider collider =
                targetBuffer[i];

            if (collider == null)
            {
                continue;
            }

            detectedTargets.Add(
                collider
            );
        }

        if (detectedTargets.Count == 0)
        {
            return null;
        }

        // 対象が1つだけならそのまま。
        if (detectedTargets.Count == 1)
        {
            return GetTargetTransform(
                detectedTargets[0]
            );
        }

        // 複数ある場合。
        return FindBestTarget(
            worldPosition
        );
    }

    /// <summary>
    /// 複数対象から目的地を選択する。
    /// </summary>
    private Transform FindBestTarget(
        Vector3 touchPosition)
    {
        Collider bestCollider = null;

        float bestDistance =
            float.MaxValue;

        for (
            int i = 0;
            i < detectedTargets.Count;
            i++
        )
        {
            Collider collider =
                detectedTargets[i];

            if (collider == null)
            {
                continue;
            }

            Vector3 targetPosition =
                GetTargetPosition(
                    collider
                );

            float distance =
                Vector3.Distance(
                    touchPosition,
                    targetPosition
                );

            if (
                selectNearestTarget &&
                distance < bestDistance
            )
            {
                bestDistance =
                    distance;

                bestCollider =
                    collider;
            }
        }

        if (bestCollider == null)
        {
            return null;
        }

        return GetTargetTransform(
            bestCollider
        );
    }

    /// <summary>
    /// Colliderから目的地Transformを取得。
    /// </summary>
    private Transform GetTargetTransform(
        Collider collider)
    {
        if (collider == null)
        {
            return null;
        }

        return collider.transform;
    }

    /// <summary>
    /// 対象オブジェクトの位置を取得。
    /// </summary>
    private Vector3 GetTargetPosition(
        Collider collider)
    {
        if (collider == null)
        {
            return Vector3.zero;
        }

        if (useTargetCenter)
        {
            return collider.bounds.center;
        }

        return collider.transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugRadius)
        {
            return;
        }

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            LastTouchPosition,
            lightRadius
        );
    }
}