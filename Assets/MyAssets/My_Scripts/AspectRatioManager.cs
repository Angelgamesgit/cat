using UnityEngine;
using System;

public class AspectRatioManager : MonoBehaviour
{
    public static AspectRatioManager Instance { get; private set; }

    // 画面タイプを定義
    public enum ScreenType { Wide, Normal, Tall }

    // 現在の画面タイプ
    public ScreenType CurrentScreenType { get; private set; }

    // 画面タイプが変更されたことを通知するイベント
    public static event Action<ScreenType> OnScreenTypeChanged;

    // 各画面タイプの境界となるアスペクト比（幅 / 高さ）
    [SerializeField, Tooltip("この値より大きいアスペクト比は「横長」と判定")]
    private float wideScreenThreshold = 1.85f; // (例: 19.5:9 => 2.16)

    [SerializeField, Tooltip("この値より小さいアスペクト比は「縦長」と判定")]
    private float tallScreenThreshold = 0.5f; // (例: 9:19.5 => 0.46)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CheckScreenType();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 必要であれば画面解像度の変更を検知するためにUpdateに入れる
    // void Update() { CheckScreenType(); }

    private void CheckScreenType()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        ScreenType newScreenType;

        if (currentAspect > wideScreenThreshold)
        {
            newScreenType = ScreenType.Wide;
        }
        else if (currentAspect < tallScreenThreshold)
        {
            newScreenType = ScreenType.Tall;
        }
        else
        {
            newScreenType = ScreenType.Normal;
        }

        if (newScreenType != CurrentScreenType)
        {
            CurrentScreenType = newScreenType;
            Debug.Log($"画面タイプが変更されました: {CurrentScreenType} (Aspect: {currentAspect})");
            OnScreenTypeChanged?.Invoke(CurrentScreenType);
        }
    }
}