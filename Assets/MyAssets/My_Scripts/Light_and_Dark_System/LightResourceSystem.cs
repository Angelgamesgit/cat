using System;
using UnityEngine;

/// <summary>
/// ゲーム中の光残量を管理するシステム。
///
/// 初期値       : 100%
/// 自然消費     : 18000フレームで100% → 0%
/// タッチ消費   : 900フレーム分
/// 0になった時  : OnLightDepletedを発火
///
/// ※フレーム単位で管理するため、Time.deltaTimeは使用しない。
/// </summary>
public class LightResourceSystem : MonoBehaviour
{
    [Header("Light Settings")]

    [Tooltip("ゲーム開始時の光残量")]
    [SerializeField]
    [Range(0f, 1f)]
    private float initialLightRate = 1f;

    [Tooltip("このフレーム数で100%→0%になる")]
    [SerializeField]
    [Min(1)]
    private int totalDrainFrames = 18000;

    [Tooltip("タッチ1回で消費するフレーム数")]
    [SerializeField]
    [Min(0)]
    private int touchCostFrames = 900;

    [Header("Debug")]

    [Tooltip("開始時に自動的に光の消費を開始する")]
    [SerializeField]
    private bool autoStart = true;

    [Tooltip("Consoleへ残量情報を表示する")]
    [SerializeField]
    private bool debugLog = false;

    // 現在の残量
    private float lightRate;

    // 現在までに消費したフレーム数
    private int consumedFrames;

    // すでに0になったことを通知したか
    private bool depletedNotified;
    /// <summary>
    /// 光残量が0になった瞬間に呼ばれる。
    /// </summary>
    public event Action OnLightDepleted;

    /// <summary>
    /// 光残量が変更されたときに呼ばれる。
    /// 引数は0～1。
    /// </summary>
    public event Action<float> OnLightChanged;
    GameSystem gameSystem;

    /// <summary>
    /// 光残量。
    /// 0～1。
    /// </summary>
    public float LightRate
    {
        get
        {
            return lightRate;
        }
    }

    /// <summary>
    /// 光残量。
    /// 0～100。
    /// </summary>
    public float LightPercentage
    {
        get
        {
            return lightRate * 100f;
        }
    }



    /// <summary>
    /// 光が0になっているか。
    /// </summary>
    public bool IsDepleted
    {
        get
        {
            return lightRate <= 0f;
        }
    }

    /// <summary>
    /// 現在までに消費したフレーム数。
    /// </summary>
    public int ConsumedFrames
    {
        get
        {
            return consumedFrames;
        }
    }

    /// <summary>
    /// 残り何フレームで自然消費が完了するか。
    /// </summary>
    public int RemainingFrames
    {
        get
        {
            return Mathf.Max(
                0,
                totalDrainFrames - consumedFrames
            );
        }
    }

    private void Awake()
    {
        ResetLight();
    }

    private void Start()
    {
        gameSystem = GameSystem.Instance;
        if (autoStart)
        {
            StartLight();
        }
    }

    private void Update()
    {
        if (!gameSystem.isPlaying)
        {
            return;
        }
        ConsumeOneFrame();
    }

    /// <summary>
    /// 光を1フレーム分消費する。
    /// </summary>
    private void ConsumeOneFrame()
    {
        if (lightRate <= 0f)
        {
            DepleteLight();
            return;
        }

        consumedFrames++;
        lightRate =
            1f -
            (float)consumedFrames /
            totalDrainFrames;

        lightRate =
            Mathf.Clamp01(lightRate);

        OnLightChanged?.Invoke(
            lightRate
        );

        if (debugLog)
        {
            Debug.Log(
                $"Light : {LightPercentage:F2}% " +
                $"({consumedFrames}/" +
                $"{totalDrainFrames} frames)"
            );
        }
    }

    /// <summary>
    /// タッチによる光の消費。
    ///
    /// 900フレーム分を消費する。
    /// </summary>
    public bool UseLightByTouch()
    {
        return UseLightFrames(touchCostFrames);
    }

    /// <summary>
    /// 指定したフレーム数分の光を消費する。
    /// </summary>
    bool UseLightFrames(int frames)
    {
        if (frames <= 0)
        {
            return true;
        }

        if (lightRate <= 0f)
        {
            DepleteLight();
            return false;
        }

        consumedFrames += frames;

        consumedFrames =
            Mathf.Clamp(
                consumedFrames,
                0,
                totalDrainFrames
            );

        lightRate =
            1f -
            (float)consumedFrames /
            totalDrainFrames;

        lightRate =
            Mathf.Clamp01(
                lightRate
            );

        OnLightChanged?.Invoke(
            lightRate
        );

        if (debugLog)
        {
            Debug.Log(
                $"Light Touch Cost : " +
                $"{frames} frames / " +
                $"{LightPercentage:F2}% remaining"
            );
        }

        if (lightRate <= 0f)
        {
            lightRate = 0f;

            DepleteLight();

            return false;
        }

        return true;
    }

    /// <summary>
    /// 光を0にする。
    /// </summary>
    private void DepleteLight()
    {
        lightRate = 0f;
        if (depletedNotified)
        {
            return;
        }
        GameSystem.Instance.GameEnd();
        depletedNotified = true;

        OnLightChanged?.Invoke(
            0f
        );

        OnLightDepleted?.Invoke();

        if (debugLog)
        {
            Debug.Log(
                "【LightResourceSystem】" +
                "光残量が0になりました。"
            );
        }

        gameSystem.isPlaying = false;
    }

    /// <summary>
    /// 光の消費を開始。
    /// </summary>
    public void StartLight()
    {
        if (lightRate <= 0f)
        {
            return;
        }
    }

    /// <summary>
    /// 光の消費を停止。
    /// </summary>
    public void StopLight()
    {
        gameSystem.isPlaying = false;
    }

    /// <summary>
    /// 光残量を100%に戻す。
    /// </summary>
    public void ResetLight()
    {
        lightRate =
            Mathf.Clamp01(
                initialLightRate
            );

        consumedFrames = 0;

        depletedNotified = false;
        OnLightChanged?.Invoke(
            lightRate
        );
    }

    /// <summary>
    /// 光残量を指定割合に設定する。
    /// 0～1。
    /// </summary>
    public void SetLightRate(
        float rate)
    {
        lightRate =
            Mathf.Clamp01(rate);

        consumedFrames =
            Mathf.RoundToInt(
                (1f - lightRate) *
                totalDrainFrames
            );

        depletedNotified =
            lightRate <= 0f;

        OnLightChanged?.Invoke(
            lightRate
        );

        if (lightRate <= 0f)
        {
            DepleteLight();
        }
    }
}