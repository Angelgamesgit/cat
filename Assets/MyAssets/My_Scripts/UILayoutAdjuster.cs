using UnityEngine;
using DG.Tweening; // アニメーションさせる場合（任意）

public class UILayoutAdjuster : MonoBehaviour
{
    // 各画面タイプごとのレイアウト設定を保持するクラス
    [System.Serializable]
    public class LayoutSettings
    {
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale = Vector3.one;
    }

    [Header("レイアウト設定")]
    [SerializeField] private LayoutSettings wideLayout;
    [SerializeField] private LayoutSettings normalLayout;
    [SerializeField] private LayoutSettings tallLayout;
    
    [Header("アニメーション")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationDuration = 0.3f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // 起動時と、イベント発生時にレイアウトを調整する
        AspectRatioManager.OnScreenTypeChanged += AdjustLayout;
        AdjustLayout(AspectRatioManager.Instance.CurrentScreenType);
    }

    private void OnDisable()
    {
        // オブジェクトが無効になる際にイベント購読を解除
        AspectRatioManager.OnScreenTypeChanged -= AdjustLayout;
    }

    private void AdjustLayout(AspectRatioManager.ScreenType screenType)
    {
        LayoutSettings targetLayout;

        switch (screenType)
        {
            case AspectRatioManager.ScreenType.Wide:
                targetLayout = wideLayout;
                break;
            case AspectRatioManager.ScreenType.Tall:
                targetLayout = tallLayout;
                break;
            default: // Normal
                targetLayout = normalLayout;
                break;
        }

        ApplyLayout(targetLayout);
    }

    private void ApplyLayout(LayoutSettings layout)
    {
        if (useAnimation && Application.isPlaying)
        {
            // DOTweenを使ってスムーズにレイアウトを変更
            rectTransform.DOAnchorPos(layout.anchoredPosition, animationDuration).SetEase(Ease.OutCubic);
            rectTransform.DOSizeDelta(layout.sizeDelta, animationDuration).SetEase(Ease.OutCubic);
            transform.DOScale(layout.localScale, animationDuration).SetEase(Ease.OutCubic);
        }
        else
        {
            // 即座にレイアウトを適用
            rectTransform.anchoredPosition = layout.anchoredPosition;
            rectTransform.sizeDelta = layout.sizeDelta;
            transform.localScale = layout.localScale;
        }
    }
}