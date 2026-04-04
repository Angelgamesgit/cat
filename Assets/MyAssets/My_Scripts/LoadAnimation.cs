using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System;
using TMPro;


public class LoadAnimation : MonoBehaviour
{
    public Sprite rocketSprite;
    public Sprite footprintSprite;

    // --- プライベート変数 ---
    private GameObject canvasGo;
    private Image dimBackground;
    private Image rocketImage;
    private Image blackCover;

    public void Loadshow(Action onComplete)
    {
         CreateUI(); // UIを生成
        // アニメーションシーケンスを作成
        var sequence = DOTween.Sequence();

        sequence.Append(dimBackground.DOFade(0.6f, 0.3f)) // ①背景を少し暗くする
                .AppendCallback(() => rocketImage.gameObject.SetActive(true));
        // ロケットのパスとスケールアニメーションを同時に再生
        Sequence rocketSequence = CreateRocketPathTween_EllipticalLanding();
        sequence.Join(rocketSequence).OnComplete(() =>
                {
                    onComplete?.Invoke(); // ④コールバックを呼び出す
                    
                }); ;
        sequence.Append(blackCover.transform.DOScale(1f, 0.3f).SetEase(Ease.InCubic))// ③黒い画面が広がる
        .AppendCallback(() =>
        {
            // ロケットの着地後に足跡アニメーションを再生
            PlayFootprintLoadingAnimation();
        }
         );
    }

    // ロケットが楕円を描いて着地するアニメーション
    private Sequence CreateRocketPathTween_EllipticalLanding()
    {
        // 画面サイズ・中心座標
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 楕円コースの制御点（右→中央上→下→中央）
        Vector3[] path = new Vector3[]
        {
            new Vector3(screenCenter.x , screenCenter.y, 0),                // 画面右外
            new Vector3(screenCenter.x, screenCenter.y + Screen.height / 4f, 0), // 画面中央より上
            new Vector3(-(Screen.width /5f ) + screenCenter.x, screenCenter.y, 0), // 画面左外
            new Vector3(screenCenter.x, screenCenter.y - Screen.height / 6f, 0), // 画面中央より下
            new Vector3(screenCenter.x + (Screen.width / 8f), screenCenter.y - (Screen.height / 8f), 0), // 画面中央より下
            new Vector3(screenCenter.x, screenCenter.y, 0)                       // 画面中央
        };
        // パス座標を画面中央基準に変換
       
        // ロケット初期設定
        rocketImage.transform.localScale = Vector3.one * 0.5f;
        rocketImage.rectTransform.localRotation = Quaternion.identity;
        rocketImage.rectTransform.anchoredPosition = new Vector2(0, -Screen.height / 2f - 150); // 画面下外からスタート

        // パス移動Tween（楕円を描く）
        var pathTween = rocketImage.rectTransform.DOPath(path, 1f, PathType.CatmullRom, PathMode.TopDown2D)
            .SetEase(Ease.OutQuad);

        pathTween.OnUpdate(() =>
        {
            // 進行方向にロケットの角度を合わせる
            Vector3 currentPos = rocketImage.rectTransform.position;
            float t = pathTween.ElapsedPercentage();
            if (t > 0f)
            {
                // 経路上の前の位置を取得（線形補間で近似）
                float prevT = Mathf.Max(0, t - 0.01f);
                int segmentCount = path.Length - 1;
                float segmentLength = 1f / segmentCount;
                int prevSegment = Mathf.FloorToInt(prevT / segmentLength);
                int currSegment = Mathf.FloorToInt(t / segmentLength);

                prevSegment = Mathf.Clamp(prevSegment, 0, segmentCount - 1);
                currSegment = Mathf.Clamp(currSegment, 0, segmentCount - 1);

                float prevSegmentT = (prevT - segmentLength * prevSegment) / segmentLength;
                float currSegmentT = (t - segmentLength * currSegment) / segmentLength;

                Vector3 prevPathPos = Vector3.Lerp(path[prevSegment], path[prevSegment + 1], prevSegmentT);
                Vector3 currPathPos = Vector3.Lerp(path[currSegment], path[currSegment + 1], currSegmentT);

                Vector3 dir = currPathPos - prevPathPos;
                if (dir.sqrMagnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    rocketImage.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                if (currSegment == segmentCount - 1) // 最後のセグメントでは角度を固定
                {
                    rocketImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 0); // 着地時は水平に
                }
            }
        });

        // 大きさアニメーション（楕円コース中は少し大きく、着地時に縮小→ぷるっと）
        var scaleTween = DOTween.Sequence();
        scaleTween.Append(rocketImage.transform.DOScale(1.2f, 0.7f).SetEase(Ease.OutBack)) // 楕円コース中に拡大
                  .Append(rocketImage.transform.DOScale(0.9f, 0.3f).SetEase(Ease.InCubic)); // 着地時に縮小
                  

        // 着地時の揺れ
        var shakeTween = rocketImage.rectTransform.DOShakeAnchorPos(0.2f, 12, 8, 90, false, true)
            .SetDelay(0.1f);

        // まとめて返す
        var groupSequence = DOTween.Sequence();
        groupSequence.Append(pathTween)
                     .Join(scaleTween)
                     .Append(shakeTween);

        return groupSequence;
    }
    void PlayFootprintLoadingAnimation()
    {
        // --- Loadingテキスト（アニメーションは最初のみ） ---
        if (canvasGo.transform.Find("LoadingText") == null)
        {
            GameObject loadingTextObj = new GameObject("LoadingText", typeof(TextMeshProUGUI));
            loadingTextObj.transform.SetParent(canvasGo.transform, false);
            TextMeshProUGUI loadingText = loadingTextObj.GetComponent<TextMeshProUGUI>();
            loadingText.text = "Loading";
            loadingText.fontSize = 64;
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.color = new Color(1, 1, 1, 0.9f);
            RectTransform loadingTextRect = loadingText.rectTransform;
            loadingTextRect.anchoredPosition = new Vector2(0, rocketImage.rectTransform.anchoredPosition.y - 250f);
            loadingTextRect.sizeDelta = new Vector2(600, 120);
        }

        // --- 猫の足跡アニメーション（…の代わり） ---
        // 足跡のサイズ・位置
        float[] sizes = { 100f, 150f, 200f };
        float spacing = 215f;
        float baseY = rocketImage.rectTransform.anchoredPosition.y - 250f;
        float centerX = rocketImage.rectTransform.anchoredPosition.x;

        List<Image> pawprints = new List<Image>();

        // 足跡を生成
        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject($"Footprint_{i}", typeof(Image));
            go.transform.SetParent(canvasGo.transform, false);
            Image img = go.GetComponent<Image>();
            img.sprite = footprintSprite;
            img.color = new Color(1, 1, 1, 0); // 最初は透明
            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(sizes[i], sizes[i]);
            rt.anchoredPosition = new Vector2(centerX + (i - 1) * spacing, baseY - 175f); // Loadingテキストの下
            rt.localScale = Vector3.zero;
            pawprints.Add(img);
        }

        Sequence seq = DOTween.Sequence();
        float singleAnimTime = 0.7f;
        float popTime = 0.25f;
        float fadeInTime = 0.15f;
        float delayBetween = 0.25f;

        for (int i = 0; i < pawprints.Count; i++)
        {
            Image img = pawprints[i];
            seq.Append(img.DOFade(1f, fadeInTime))
               .Join(img.rectTransform.DOScale(1.2f, popTime).SetEase(Ease.OutBack))
               .Append(img.rectTransform.DOScale(1f, singleAnimTime - popTime).SetEase(Ease.OutCubic));
            if (i < pawprints.Count - 1)
                seq.AppendInterval(delayBetween);
        }

        seq.AppendInterval(0.5f);
        foreach (var img in pawprints)
        {
            seq.Join(img.rectTransform.DOScale(0f, 0.4f).SetEase(Ease.InBack));
        }

        seq.AppendInterval(0.2f);
        seq.OnComplete(() =>
        {
            foreach (var img in pawprints)
            {
                if (img != null) Destroy(img.gameObject);
            }
            PlayFootprintLoadingAnimation(); // 足跡だけループ
        });
    }  public void Hide(Action onComplete = null)
    {

        if (canvasGo == null)
        {
            Debug.LogWarning("LoadAnimationのキャンバスが存在しません。");
            onComplete?.Invoke();
            return;
        }

        // 黒い背景を縮小
        blackCover.transform.DOScale(0f, 0.3f).SetEase(Ease.InCubic);
       
    // 足跡とローディングテキストを探して消す
        foreach (Transform child in canvasGo.transform)
        {
            if (child.name.StartsWith("Footprint_"))
            {
                var img = child.GetComponent<Image>();
                if (img != null)
                    img.DOFade(0f, 0.2f).OnComplete(() => Destroy(child.gameObject));
            }
            else if (child.name == "LoadingText")
            {
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.DOFade(0f, 0.2f).OnComplete(() => Destroy(child.gameObject));
            }
        } // ロード終了演出
        // 1. 黒い背景をロケットを中心に小さくする
        // 2. ロケットが画面上に向かって打ち上がる
        // 3. 半透明の背景が透明にフェードし、キャンバスごと消滅
        // 4. 全体で約0.75秒
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);


        // 曲線的な動き（Ease.OutBackでバウンド感、DOAnchorPosで移動）
        var sequence = DOTween.Sequence();

        sequence.Append(blackCover.transform.DOScale(0f, 0.3f).SetEase(Ease.OutCubic)) // ①黒い画面が縮小
                .Join(rocketImage.rectTransform.DOAnchorPosY(Screen.height + 400f, 0.7f).SetEase(Ease.InBack)) // ②ロケットが上に飛んでいく
                .Append(dimBackground.DOFade(0f, 0.15f)) // ③背景の暗転を消す
                .OnComplete(() =>
                {
                    Destroy(gameObject); // ④オブジェクトを全て破棄
                    onComplete?.Invoke();
                });
        // The total duration is determined by the appended tweens and intervals.
    }

    private void CreateUI()
    {
        canvasGo = new GameObject("LoadingCanvas");
        canvasGo.transform.SetParent(transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // 各Image要素を生成
        dimBackground = CreateImage("DimBackground", canvasGo.transform, null, new Color(0, 0, 0, 0));
        StretchToFullScreen(dimBackground.rectTransform);

        blackCover = CreateImage("BlackCover", canvasGo.transform, null, Color.black);
        blackCover.rectTransform.anchoredPosition = Vector2.zero;
        blackCover.rectTransform.sizeDelta = new Vector2(2500, 2500); // 画面より十分大きく
        blackCover.transform.localScale = Vector3.zero;

        rocketImage = CreateImage("Rocket", canvasGo.transform, rocketSprite, Color.white);
        rocketImage.rectTransform.sizeDelta = new Vector2(400,400);
        rocketImage.rectTransform.anchoredPosition = new Vector2(0, -Screen.height / 2f - 150); // 画面下外からスタート
        rocketImage.rectTransform.localRotation = Quaternion.identity;
    }

       private Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private void StretchToFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }
}
