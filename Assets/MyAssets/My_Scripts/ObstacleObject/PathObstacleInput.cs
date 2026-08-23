using UnityEngine;

/// <summary>
/// PCのマウスクリックとスマートフォンのタッチを検出し、
/// 現在選択されている道具を使って障害物を壊します。
/// </summary>
public class PathObstacleInput : MonoBehaviour
{
    [Header("参照")]

    [Tooltip("Raycastに使用するカメラ")]
    [SerializeField]
    private Camera targetCamera;

    [Tooltip("現在使用している道具を管理するToolManager")]
    [SerializeField]
    private ToolManager toolManager;

    [Header("Raycast設定")]

    [Tooltip("障害物を検出するLayer")]
    [SerializeField]
    private LayerMask obstacleLayer = ~0;

    [Header("デバッグ")]

    [SerializeField]
    private bool showDebugLog = false;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError(
                "PathObstacleInput: Cameraが設定されていません。"
            );
        }

        if (toolManager == null)
        {
            Debug.LogError(
                "PathObstacleInput: ToolManagerが設定されていません。"
            );
        }
    }

    private void Update()
    {
        HandleMouseInput();

        HandleTouchInput();
    }

    /// <summary>
    /// PCのマウスクリックを処理します。
    /// </summary>
    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        TryHitObstacle(Input.mousePosition);
    }

    /// <summary>
    /// スマートフォンのタッチを処理します。
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
        {
            return;
        }

        TryHitObstacle(touch.position);
    }

    /// <summary>
    /// 指定した画面座標から障害物を探します。
    /// </summary>
    private void TryHitObstacle(Vector2 screenPosition)
    {
        if (targetCamera == null)
        {
            return;
        }

        if (toolManager == null)
        {
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                obstacleLayer))
        {
            return;
        }

        ObstacleObject obstacle =
            hit.collider.GetComponentInParent<ObstacleObject>();

        if (obstacle == null)
        {
            return;
        }

        ToolType currentTool = toolManager.CurrentTool;

        if (showDebugLog)
        {
            Debug.Log(
                $"障害物: {obstacle.name} / " +
                $"現在の道具: {currentTool} / " +
                $"必要な道具: {obstacle.data.requiredTool}"
            );
        }

        // 現在選択している道具を渡して破壊処理
        obstacle.Hit(currentTool);
    }
}