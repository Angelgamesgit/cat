using UnityEngine;

/// <summary>
/// 球体表面の探索状況をTextureとして管理する。
///
/// 黒   = 未探索
/// 白   = プレイヤーが通過した場所
///
/// UnityのLightを大量生成せず、
/// 1枚のTextureで探索済み領域を管理する。
/// </summary>
public class SphereExplorationTexture : MonoBehaviour
{
    [Header("Sphere")]

    [Tooltip("探索対象となる球体")]
    [SerializeField]
    private Renderer sphereRenderer;

    [Tooltip("球体のTransform")]
    [SerializeField]
    private Transform sphereTransform;

    [Header("Exploration Texture")]

    [Tooltip("探索Textureの解像度")]
    [SerializeField]
    [Range(128, 2048)]
    private int textureSize = 512;

    [Tooltip("探索した場所の光の強さ")]
    [SerializeField]
    [Range(0f, 1f)]
    private float revealIntensity = 1f;

    [Tooltip("猫の周囲をどのくらい明るくするか")]
    [SerializeField]
    [Range(0.001f, 0.5f)]
    private float revealRadius = 0.04f;

    [Header("Shader")]

    [Tooltip("球体Materialへ自動的にTextureを渡す")]
    [SerializeField]
    private bool automaticallyApplyTexture = true;

    private Texture2D explorationTexture;

    private Color32[] pixels;

    private MaterialPropertyBlock propertyBlock;

    private bool initialized;

    public Texture2D ExplorationTexture
    {
        get
        {
            return explorationTexture;
        }
    }

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化。
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        if (sphereRenderer == null)
        {
            Debug.LogError(
                "SphereExplorationTexture : " +
                "Sphere Rendererが設定されていません。"
            );

            return;
        }

        if (sphereTransform == null)
        {
            sphereTransform =
                sphereRenderer.transform;
        }

        explorationTexture =
            new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.R8,
                false,
                true
            );

        explorationTexture.name =
            "Sphere Exploration Texture";

        explorationTexture.wrapMode =
            TextureWrapMode.Repeat;

        explorationTexture.filterMode =
            FilterMode.Bilinear;

        pixels =
            new Color32[
                textureSize *
                textureSize
            ];

        propertyBlock =
            new MaterialPropertyBlock();

        ClearExploration();

        initialized = true;

        if (automaticallyApplyTexture)
        {
            ApplyTextureToSphere();
        }
    }

    /// <summary>
    /// 全てを暗闇に戻す。
    /// </summary>
    public void ClearExploration()
    {
        if (pixels == null)
        {
            pixels =
                new Color32[
                    textureSize *
                    textureSize
                ];
        }

        Color32 darkness =
            new Color32(
                0,
                0,
                0,
                255
            );

        for (
            int i = 0;
            i < pixels.Length;
            i++
        )
        {
            pixels[i] =
                darkness;
        }

        if (explorationTexture != null)
        {
            explorationTexture.SetPixels32(
                pixels
            );

            explorationTexture.Apply(
                false,
                false
            );
        }
    }

    /// <summary>
    /// プレイヤーが現在いる場所を明るくする。
    /// </summary>
    public void RevealPlayerPosition(
        Vector3 worldPosition)
    {
        RevealWorldPosition(
            worldPosition,
            revealRadius,
            revealIntensity
        );
    }

    /// <summary>
    /// 指定したワールド座標周辺を明るくする。
    /// </summary>
    public void RevealWorldPosition(
        Vector3 worldPosition,
        float radius,
        float intensity)
    {
        if (!initialized)
        {
            Initialize();
        }

        if (explorationTexture == null)
        {
            return;
        }

        Vector2 uv =
            WorldPositionToUV(
                worldPosition
            );

        RevealUV(
            uv,
            radius,
            intensity
        );
    }

    /// <summary>
    /// UV座標を中心として円形に探索領域を追加する。
    /// </summary>
    private void RevealUV(
        Vector2 uv,
        float radius,
        float intensity)
    {
        int centerX =
            Mathf.RoundToInt(
                uv.x * textureSize
            );

        int centerY =
            Mathf.RoundToInt(
                uv.y * textureSize
            );

        int pixelRadius =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    radius * textureSize
                )
            );

        int minX =
            centerX - pixelRadius;

        int maxX =
            centerX + pixelRadius;

        int minY =
            centerY - pixelRadius;

        int maxY =
            centerY + pixelRadius;

        for (
            int y = minY;
            y <= maxY;
            y++
        )
        {
            if (
                y < 0 ||
                y >= textureSize
            )
            {
                continue;
            }

            for (
                int x = minX;
                x <= maxX;
                x++
            )
            {
                int wrappedX =
                    x % textureSize;

                if (wrappedX < 0)
                {
                    wrappedX +=
                        textureSize;
                }

                float dx =
                    x - centerX;

                float dy =
                    y - centerY;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                if (
                    distance >
                    pixelRadius
                )
                {
                    continue;
                }

                float normalizedDistance =
                    distance /
                    pixelRadius;

                float falloff =
                    1f -
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        normalizedDistance
                    );

                float value =
                    falloff *
                    intensity;

                int index =
                    y *
                    textureSize +
                    wrappedX;

                byte currentValue =
                    pixels[index].r;

                byte newValue =
                    (byte)(
                        Mathf.Clamp01(
                            value
                        ) * 255f
                    );

                // 一度明るくなった場所を
                // 暗くしない。
                if (
                    newValue >
                    currentValue
                )
                {
                    pixels[index] =
                        new Color32(
                            newValue,
                            newValue,
                            newValue,
                            255
                        );
                }
            }
        }

        explorationTexture.SetPixels32(
            pixels
        );

        explorationTexture.Apply(
            false,
            false
        );
    }

    /// <summary>
    /// ワールド座標を球体のUV座標へ変換する。
    /// </summary>
    public Vector2 WorldPositionToUV(
        Vector3 worldPosition)
    {
        if (sphereTransform == null)
        {
            return Vector2.zero;
        }

        Vector3 direction =
            worldPosition -
            sphereTransform.position;

        direction =
            sphereTransform
                .InverseTransformDirection(
                    direction
                );

        direction.Normalize();

        float longitude =
            Mathf.Atan2(
                direction.z,
                direction.x
            );

        float latitude =
            Mathf.Asin(
                Mathf.Clamp(
                    direction.y,
                    -1f,
                    1f
                )
            );

        float u =
            longitude /
            (Mathf.PI * 2f);

        u += 0.5f;

        float v =
            latitude /
            Mathf.PI;

        v += 0.5f;

        return new Vector2(
            u,
            v
        );
    }

    /// <summary>
    /// 球体Materialへ探索Textureを渡す。
    /// </summary>
    public void ApplyTextureToSphere()
    {
        if (
            sphereRenderer == null ||
            explorationTexture == null
        )
        {
            return;
        }

        sphereRenderer.GetPropertyBlock(
            propertyBlock
        );

        propertyBlock.SetTexture(
            "_ExplorationTexture",
            explorationTexture
        );

        sphereRenderer.SetPropertyBlock(
            propertyBlock
        );
    }

    /// <summary>
    /// 指定位置が探索済みか取得。
    ///
    /// ※移動制限には使用しない。
    /// 必要な場合の情報取得用。
    /// </summary>
    public bool IsExplored(
        Vector3 worldPosition)
    {
        if (
            explorationTexture == null
        )
        {
            return false;
        }

        Vector2 uv =
            WorldPositionToUV(
                worldPosition
            );

        float value =
            explorationTexture
                .GetPixelBilinear(
                    uv.x,
                    uv.y
                ).r;

        return value > 0.05f;
    }

    /// <summary>
    /// 現在の球体を変更した場合に使用する。
    /// GameSystem.SphereSet()などから呼び出せる。
    /// </summary>
    public void SetSphere(
        Renderer newRenderer)
    {
        sphereRenderer =
            newRenderer;

        if (newRenderer != null)
        {
            sphereTransform =
                newRenderer.transform;
        }

        ApplyTextureToSphere();
    }
}