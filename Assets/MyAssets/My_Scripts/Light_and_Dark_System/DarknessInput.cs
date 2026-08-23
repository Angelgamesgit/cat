using UnityEngine;

/// <summary>
/// PCのクリック、スマートフォンのタッチから
/// 3D空間上に光を生成します。
/// </summary>
public class DarknessInput : MonoBehaviour
{
    [Header("カメラ")]

    [SerializeField]
    private Camera targetCamera;

    [Header("光源")]

    [SerializeField]
    private LightSource lightPrefab;

    [SerializeField]
    private LightType currentLightType =
        LightType.Small;

    [Header("Raycast")]

    [Tooltip("光を配置できる地面のLayer")]
    [SerializeField]
    private LayerMask groundLayer;

    [Tooltip("Raycastの最大距離")]
    [SerializeField]
    private float rayDistance = 1000f;

    [Header("生成位置")]

    [Tooltip("地面から光源を上げる高さ")]
    [SerializeField]
    private float heightOffset = 0.05f;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleMouseInput();

        HandleTouchInput();
    }

    /// <summary>
    /// PC入力。
    /// </summary>
    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        TryCreateLight(
            Input.mousePosition
        );
    }

    /// <summary>
    /// スマホ入力。
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }

        Touch touch =
            Input.GetTouch(0);

        if (touch.phase !=
            TouchPhase.Began)
        {
            return;
        }

        TryCreateLight(
            touch.position
        );
    }

    /// <summary>
    /// 画面座標から3D空間へRayを飛ばします。
    /// </summary>
    private void TryCreateLight(
        Vector2 screenPosition)
    {
        if (targetCamera == null)
        {
            return;
        }

        if (lightPrefab == null)
        {
            Debug.LogError(
                "DarknessInput: " +
                "LightPrefabが設定されていません。"
            );

            return;
        }

        Ray ray =
            targetCamera.ScreenPointToRay(
                screenPosition
            );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                rayDistance,
                groundLayer))
        {
            return;
        }

        Vector3 position =
            hit.point;

        position.y += heightOffset;

        CreateLight(position);
    }

    /// <summary>
    /// 指定した3D座標に光を生成。
    /// </summary>
    private void CreateLight(
        Vector3 position)
    {
        LightSource lightSource =
            Instantiate(
                lightPrefab,
                position,
                Quaternion.identity
            );

        lightSource.SetLightType(
            currentLightType
        );
    }

    /// <summary>
    /// 使用する光を変更。
    /// UIボタンから呼び出せます。
    /// </summary>
    public void SelectLightType(
        LightType type)
    {
        currentLightType = type;
    }

    public LightType CurrentLightType =>
        currentLightType;
}