using UnityEngine;

/// <summary>
/// プレイヤーが暗闇へ移動することを禁止します。
/// </summary>
public class LightMovementRestriction : MonoBehaviour
{
    [Header("参照")]

    [SerializeField]
    private DarknessManager darknessManager;

    [Header("設定")]

    [Tooltip("移動先だけでなく、プレイヤー自身も光の中にいる必要があるか")]
    [SerializeField]
    private bool requireCurrentPositionLit = true;

    private void Awake()
    {
        if (darknessManager == null)
        {
            darknessManager =
                FindFirstObjectByType<DarknessManager>();
        }
    }

    /// <summary>
    /// 移動可能か判定。
    /// </summary>
    public bool CanMoveTo(
        Vector3 targetPosition)
    {
        if (darknessManager == null)
        {
            return false;
        }

        // 現在位置も光の中にいる必要がある場合
        if (requireCurrentPositionLit)
        {
            if (!darknessManager.IsPositionLit(
                    transform.position))
            {
                return false;
            }
        }

        // 移動先が光の中か
        return darknessManager.IsPositionLit(
            targetPosition
        );
    }

    /// <summary>
    /// 移動先が光の中なら実際に移動。
    /// </summary>
    public bool TryMoveTo(
        Vector3 targetPosition)
    {
        if (!CanMoveTo(targetPosition))
        {
            return false;
        }

        transform.position =
            targetPosition;

        return true;
    }

    /// <summary>
    /// 現在位置が明るいか。
    /// </summary>
    public bool IsCurrentPositionLit()
    {
        if (darknessManager == null)
        {
            return false;
        }

        return darknessManager.IsPositionLit(
            transform.position
        );
    }
}