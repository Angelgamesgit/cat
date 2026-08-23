using UnityEngine;

/// <summary>
/// 3D空間上に存在する1つの光源。
/// 光の位置・半径・種類・色・持続時間を管理します。
/// </summary>
public class LightSource : MonoBehaviour
{
    [Header("光の種類")]

    [SerializeField]
    private LightType lightType = LightType.Small;

    [Header("光の設定")]

    [Tooltip("光が届くワールド空間上の半径")]
    [SerializeField, Min(0.1f)]
    private float radius = 3f;

    [Tooltip("光の境界が暗闇へ変化する範囲")]
    [SerializeField, Min(0f)]
    private float softness = 1f;

    [Tooltip("光の強さ")]
    [SerializeField, Range(0f, 1f)]
    private float intensity = 1f;

    [Tooltip("0以下なら永続")]
    [SerializeField, Min(0f)]
    private float duration = 0f;

    [Header("光の色")]

    [SerializeField]
    private Color lightColor = Color.white;

    private DarknessManager darknessManager;

    public LightType Type => lightType;

    public float Radius => radius;

    public float Softness => softness;

    public float Intensity => intensity;

    public Color LightColor => lightColor;

    private void Awake()
    {
        ApplyLightTypeSettings();
    }

    private void OnEnable()
    {
        RegisterToManager();
    }

    private void OnDisable()
    {
        UnregisterFromManager();
    }

    private void Start()
    {
        if (duration > 0f)
        {
            Destroy(gameObject, duration);
        }
    }

    /// <summary>
    /// DarknessManagerへ登録。
    /// </summary>
    private void RegisterToManager()
    {
        if (darknessManager == null)
        {
            darknessManager =
                FindFirstObjectByType<DarknessManager>();
        }

        if (darknessManager != null)
        {
            darknessManager.RegisterLight(this);
        }
    }

    /// <summary>
    /// DarknessManagerから登録解除。
    /// </summary>
    private void UnregisterFromManager()
    {
        if (darknessManager != null)
        {
            darknessManager.UnregisterLight(this);
        }
    }

    /// <summary>
    /// 光の種類に応じた設定。
    /// 半径はワールド座標単位。
    /// </summary>
    private void ApplyLightTypeSettings()
    {
        switch (lightType)
        {
            case LightType.Small:

                radius = 3f;
                softness = 0.75f;
                intensity = 1f;

                break;

            case LightType.Medium:

                radius = 6f;
                softness = 1f;
                intensity = 1f;

                break;

            case LightType.Large:

                radius = 10f;
                softness = 1.5f;
                intensity = 1f;

                break;
        }
    }

    /// <summary>
    /// 光の種類を変更。
    /// </summary>
    public void SetLightType(LightType type)
    {
        lightType = type;

        ApplyLightTypeSettings();
    }

    /// <summary>
    /// 光の半径を変更。
    /// </summary>
    public void SetRadius(float value)
    {
        radius = Mathf.Max(0.1f, value);
    }

    /// <summary>
    /// 指定した3D座標が、この光の範囲内にあるか。
    /// </summary>
    public bool ContainsPosition(Vector3 position)
    {
        Vector3 difference =
            position - transform.position;

        // 地面上の移動を想定するためY軸を無視
        difference.y = 0f;

        float distance =
            difference.magnitude;

        return distance <= radius;
    }

    /// <summary>
    /// Shaderへ渡す光源情報。
    ///
    /// XYZ = ワールド座標
    /// W   = 半径
    /// </summary>
    public Vector4 GetShaderPositionData()
    {
        return new Vector4(
            transform.position.x,
            transform.position.y,
            transform.position.z,
            radius
        );
    }

    /// <summary>
    /// Shaderへ渡す光源カラー。
    /// </summary>
    public Vector4 GetShaderColorData()
    {
        return new Vector4(
            lightColor.r,
            lightColor.g,
            lightColor.b,
            intensity
        );
    }
}