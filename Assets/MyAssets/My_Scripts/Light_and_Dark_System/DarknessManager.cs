using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D空間上のすべての光源を管理します。
/// </summary>
public class DarknessManager : MonoBehaviour
{
    public const int MaxLights = 32;

    [Header("光源設定")]

    [SerializeField, Range(1, MaxLights)]
    private int maxLightCount = MaxLights;

    private readonly List<LightSource> lightSources =
        new List<LightSource>();

    public IReadOnlyList<LightSource> LightSources =>
        lightSources;

    private void LateUpdate()
    {
        RemoveNullLights();

        UpdateShaderData();
    }

    /// <summary>
    /// 光源を登録。
    /// </summary>
    public bool RegisterLight(
        LightSource lightSource)
    {
        if (lightSource == null)
        {
            return false;
        }

        if (lightSources.Contains(lightSource))
        {
            return true;
        }

        if (lightSources.Count >= maxLightCount)
        {
            Debug.LogWarning(
                "DarknessManager: " +
                "最大光源数に到達しています。"
            );

            return false;
        }

        lightSources.Add(lightSource);

        return true;
    }

    /// <summary>
    /// 光源を登録解除。
    /// </summary>
    public void UnregisterLight(
        LightSource lightSource)
    {
        if (lightSource == null)
        {
            return;
        }

        lightSources.Remove(lightSource);
    }

    /// <summary>
    /// 指定座標が明るいか判定。
    /// </summary>
    public bool IsPositionLit(
        Vector3 position)
    {
        RemoveNullLights();

        foreach (
            LightSource lightSource
            in lightSources)
        {
            if (lightSource.ContainsPosition(
                    position))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Shaderへ光源情報を送ります。
    /// </summary>
    private void UpdateShaderData()
    {
        int count =
            Mathf.Min(
                lightSources.Count,
                MaxLights
            );

        Vector4[] positions =
            new Vector4[MaxLights];

        Vector4[] colors =
            new Vector4[MaxLights];

        for (int i = 0; i < count; i++)
        {
            LightSource lightSource =
                lightSources[i];

            if (lightSource == null)
            {
                continue;
            }

            positions[i] =
                lightSource.GetShaderPositionData();

            colors[i] =
                lightSource.GetShaderColorData();
        }

        Shader.SetGlobalInt(
            "_DarknessLightCount",
            count
        );

        Shader.SetGlobalVectorArray(
            "_DarknessLightPositions",
            positions
        );

        Shader.SetGlobalVectorArray(
            "_DarknessLightColors",
            colors
        );
    }

    /// <summary>
    /// nullになった光源を削除。
    /// </summary>
    private void RemoveNullLights()
    {
        for (
            int i = lightSources.Count - 1;
            i >= 0;
            i--)
        {
            if (lightSources[i] == null)
            {
                lightSources.RemoveAt(i);
            }
        }
    }
}