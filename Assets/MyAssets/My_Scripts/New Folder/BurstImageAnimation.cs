using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTweenの名前空間を必ずusingします
using System.Collections.Generic; // 必要に応じて

public class BurstImageAnimation : MonoBehaviour
{
     [Header("UI設定")]
    [Tooltip("アニメーションさせるImageのプレハブ。RectTransform, Image, CanvasGroup を持つこと。")]
    public GameObject imagePrefab;

    [Tooltip("Imageをインスタンス化する親となるRectTransform（通常はCanvasなど）")]
    public RectTransform parentRectTransform;

    [Header("アニメーション基本設定")]
    [Tooltip("1つ目のImageと2つ目のImageが出現する間の遅延時間")]
    public float delayBetweenPrimaryAndSecondary = 0.15f;

    [Header("2つ目のImageの設定")]
    [Tooltip("1つ目のImageから2つ目のImageまでの距離")]
    public float secondaryImageDistance = 60f;

    [Tooltip("2つ目のImageのスケール（1つ目に対する割合、例: 0.8で80%）")]
    public float secondaryImageScaleFactor = 0.8f;

    // --- アニメーションの各フェーズの期間 (固定) ---
    private const float FadeInDuration = 0.2f;
    private const float VisibleDuration = 0.6f; // 表示維持期間
    private const float FadeOutDuration = 0.2f;
    // 合計生存期間: FadeInDuration + VisibleDuration + FadeOutDuration = 1.0秒

    private Camera uiCamera;

    void Start()
    {
        if (imagePrefab == null)
        {
            Debug.LogError("Image Prefabが設定されていません！");
            enabled = false;
            return;
        }
        if (imagePrefab.GetComponent<CanvasGroup>() == null)
        {
            Debug.LogError("Image PrefabにCanvasGroupコンポーネントがありません！このアニメーションには必須です。");
            enabled = false;
            return;
        }

        if (parentRectTransform == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                parentRectTransform = canvas.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("Parent RectTransformが設定されておらず、親Canvasも見つかりません！");
                enabled = false;
                return;
            }
        }

        Canvas rootCanvas = parentRectTransform.GetComponentInParent<Canvas>().rootCanvas;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera || rootCanvas.renderMode == RenderMode.WorldSpace)
        {
            uiCamera = rootCanvas.worldCamera;
            if (uiCamera == null && rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                uiCamera = Camera.main;
                if (uiCamera == null) Debug.LogWarning("ScreenSpace-CameraモードのCanvasにカメラが設定されておらず、メインカメラも見つかりません。");
            }
        }
    }


    public void CreatePairedBurstEffect()
    {
        if (imagePrefab == null || parentRectTransform == null) return;
        Vector2 screenPosition = Input.mousePosition;
        Vector2 tapLocalPoint; // タップ位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform, screenPosition, uiCamera, out tapLocalPoint
        );

        // 全体のエフェクトシーケンス (プライマリとセカンダリをまとめる)
        Sequence effectSequence = DOTween.Sequence();

        // --- 1つ目のImage (プライマリ) ---
        GameObject primaryObject = Instantiate(imagePrefab, parentRectTransform);
        RectTransform primaryRt = primaryObject.GetComponent<RectTransform>();
        CanvasGroup primaryCG = primaryObject.GetComponent<CanvasGroup>();

        // 初期状態
        primaryRt.localPosition = tapLocalPoint;
        float randomAngleZ = Random.Range(-20f, 20f);
        primaryRt.localEulerAngles = new Vector3(0, 0, randomAngleZ);
        primaryRt.localScale = Vector3.one; // 初期スケールは1
        primaryCG.alpha = 0f;

        // プライマリのアニメーションシーケンス
        Sequence primarySequence = DOTween.Sequence();
        primarySequence.Append(primaryCG.DOFade(1f, FadeInDuration).SetEase(Ease.OutQuad));
        // (オプション) スケールアップアニメーション
        // primaryRt.localScale = Vector3.zero;
        // primarySequence.Insert(0, primaryRt.DOScale(1f, FadeInDuration).SetEase(Ease.OutBack));
        primarySequence.AppendInterval(VisibleDuration);
        primarySequence.Append(primaryCG.DOFade(0f, FadeOutDuration).SetEase(Ease.InQuad));
        primarySequence.OnComplete(() => {
            if (primaryObject != null) Destroy(primaryObject);
        });

        effectSequence.Append(primarySequence); // マスターシーケンスに追加

        // --- 2つ目のImage (セカンダリ) ---
        GameObject secondaryObject = Instantiate(imagePrefab, parentRectTransform);
        RectTransform secondaryRt = secondaryObject.GetComponent<RectTransform>();
        CanvasGroup secondaryCG = secondaryObject.GetComponent<CanvasGroup>();

        // 2つ目のImageの位置と角度を計算
        float angleOffsetDeg;
        if (Random.value < 0.5f) // 50%の確率でどちらかの範囲を選択
        {
            angleOffsetDeg = Random.Range(30f, 60f);
        }
        else
        {
            angleOffsetDeg = Random.Range(120f, 150f);
        }

        // プライマリのローカルY軸(上)を0度としたときの、セカンダリへの方向ベクトル（プライマリ未回転時）
        // Y軸が0度の基準なので、X = Sin(angle), Y = Cos(angle)
        Vector2 relativeDirection = new Vector2(
            Mathf.Sin(angleOffsetDeg * Mathf.Deg2Rad),
            Mathf.Cos(angleOffsetDeg * Mathf.Deg2Rad)
        ).normalized;

        // プライマリの回転を考慮したオフセットを計算
        Vector2 offsetFromPrimary = Quaternion.Euler(0, 0, randomAngleZ) * relativeDirection * secondaryImageDistance;

        // 初期状態
        secondaryRt.localPosition = (Vector2)primaryRt.localPosition + offsetFromPrimary;
        secondaryRt.localEulerAngles = new Vector3(0, 0, randomAngleZ); // プライマリと同じ角度
        secondaryRt.localScale = Vector3.one * secondaryImageScaleFactor;
        secondaryCG.alpha = 0f;

        // セカンダリのアニメーションシーケンス
        Sequence secondarySequence = DOTween.Sequence();
        secondarySequence.Append(secondaryCG.DOFade(1f, FadeInDuration).SetEase(Ease.OutQuad));
        // (オプション) スケールアップアニメーション
        // secondaryRt.localScale = Vector3.zero;
        // secondarySequence.Insert(0, secondaryRt.DOScale(secondaryImageScaleFactor, FadeInDuration).SetEase(Ease.OutBack));
        secondarySequence.AppendInterval(VisibleDuration);
        secondarySequence.Append(secondaryCG.DOFade(0f, FadeOutDuration).SetEase(Ease.InQuad));
        secondarySequence.OnComplete(() => {
            if (secondaryObject != null) Destroy(secondaryObject);
        });

        // マスターシーケンスに遅延させて追加
        effectSequence.Insert(delayBetweenPrimaryAndSecondary, secondarySequence);

        effectSequence.Play();
    }
}